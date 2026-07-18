# Specification - configuration mobile pour notifications

## Objectif

Mettre en place la premiere brique serveur pour enregistrer la configuration d'un appareil mobile utilise par l'application AGRA EASY MOBILE.

Cette configuration servira ensuite a envoyer des notifications Android et iOS, tout en permettant de synchroniser les appareils entre plusieurs plateformes comme s'il s'agissait d'un seul environnement fonctionnel.

L'application mobile envoie un XML au service SOAP. Le service SOAP delegue le traitement a une classe du package `Tools`. Cette classe parse le XML, recupere l'utilisateur connecte depuis la session, puis appelle un package Oracle charge d'inserer ou de mettre a jour la configuration appareil.

## Regles retenues

- Tous les noms de champs sont en minuscules avec des mots separes par `_`.
- Le token de notification est unique globalement, sans inclure la plateforme dans la cle unique.
- La plateforme reste stockee dans la table, mais ne participe pas a l'unicite.
- Tous les profils connectes peuvent s'enregistrer.
- Le filtrage des destinataires `type = 'Client'` se fera plus tard au moment de l'envoi des notifications, pas au moment de la registration.
- `login`, `account_code` et `type` ne doivent pas venir du XML. Ils sont recuperes cote serveur via `UserProfil.GetUser()`.
- Le service SOAP doit rester minimal et deleguer toute la logique a une classe `Tools`.

## Table Oracle

Nom retenu :

```sql
easy_mobile_device_config
```

Script :

```sql
create table easy_mobile_device_config
(
    mobile_device_config_id raw(16) default sys_guid() not null,

    platform varchar2(20) not null,
    notification_provider varchar2(30) not null,
    push_token varchar2(2048) not null,
    push_token_hash raw(32) not null,

    installation_id varchar2(100),
    device_id varchar2(200),
    application_id varchar2(200),
    application_version varchar2(50),
    device_model varchar2(200),
    manufacturer varchar2(200),
    operating_system_version varchar2(100),

    login varchar2(100),
    account_code varchar2(100),
    type varchar2(50),

    registration_date_utc timestamp not null,
    last_registration_utc timestamp not null,
    is_active number(1) default 1 not null,
    created_date_utc timestamp default sys_extract_utc(systimestamp) not null,
    updated_date_utc timestamp default sys_extract_utc(systimestamp) not null,

    constraint pk_easy_mobile_dev_cfg
        primary key (mobile_device_config_id),

    constraint ck_easy_mobile_dev_active
        check (is_active in (0, 1))
);

create unique index ux_easy_mobile_dev_token
on easy_mobile_device_config(push_token_hash);
```

Les noms de contraintes et d'index restent volontairement courts pour rester compatibles avec les limites Oracle anciennes.

## Package Oracle

Nom retenu :

```sql
easy_mobile_device
```

Specification :

```sql
create or replace package easy_mobile_device as
    procedure register_device
    (
        p_platform in varchar2,
        p_notification_provider in varchar2,
        p_push_token in varchar2,
        p_push_token_hash in raw,

        p_installation_id in varchar2,
        p_device_id in varchar2,
        p_application_id in varchar2,
        p_application_version in varchar2,
        p_device_model in varchar2,
        p_manufacturer in varchar2,
        p_operating_system_version in varchar2,

        p_login in varchar2,
        p_account_code in varchar2,
        p_type in varchar2,

        p_registration_date_utc in timestamp,
        p_mobile_device_config_id out raw
    );
end easy_mobile_device;
/
```

Body :

```sql
create or replace package body easy_mobile_device as

    procedure register_device
    (
        p_platform in varchar2,
        p_notification_provider in varchar2,
        p_push_token in varchar2,
        p_push_token_hash in raw,

        p_installation_id in varchar2,
        p_device_id in varchar2,
        p_application_id in varchar2,
        p_application_version in varchar2,
        p_device_model in varchar2,
        p_manufacturer in varchar2,
        p_operating_system_version in varchar2,

        p_login in varchar2,
        p_account_code in varchar2,
        p_type in varchar2,

        p_registration_date_utc in timestamp,
        p_mobile_device_config_id out raw
    )
    is
        v_existing_date timestamp;
    begin
        begin
            select mobile_device_config_id,
                   last_registration_utc
              into p_mobile_device_config_id,
                   v_existing_date
              from easy_mobile_device_config
             where push_token_hash = p_push_token_hash
             for update;

            if p_registration_date_utc >= v_existing_date then
                update easy_mobile_device_config
                   set platform = p_platform,
                       notification_provider = p_notification_provider,
                       push_token = p_push_token,
                       installation_id = p_installation_id,
                       device_id = p_device_id,
                       application_id = p_application_id,
                       application_version = p_application_version,
                       device_model = p_device_model,
                       manufacturer = p_manufacturer,
                       operating_system_version = p_operating_system_version,
                       login = p_login,
                       account_code = p_account_code,
                       type = p_type,
                       registration_date_utc = p_registration_date_utc,
                       last_registration_utc = p_registration_date_utc,
                       is_active = 1,
                       updated_date_utc = sys_extract_utc(systimestamp)
                 where mobile_device_config_id = p_mobile_device_config_id;
            end if;

        exception
            when no_data_found then
                p_mobile_device_config_id := sys_guid();

                insert into easy_mobile_device_config
                (
                    mobile_device_config_id,
                    platform,
                    notification_provider,
                    push_token,
                    push_token_hash,
                    installation_id,
                    device_id,
                    application_id,
                    application_version,
                    device_model,
                    manufacturer,
                    operating_system_version,
                    login,
                    account_code,
                    type,
                    registration_date_utc,
                    last_registration_utc,
                    is_active,
                    created_date_utc,
                    updated_date_utc
                )
                values
                (
                    p_mobile_device_config_id,
                    p_platform,
                    p_notification_provider,
                    p_push_token,
                    p_push_token_hash,
                    p_installation_id,
                    p_device_id,
                    p_application_id,
                    p_application_version,
                    p_device_model,
                    p_manufacturer,
                    p_operating_system_version,
                    p_login,
                    p_account_code,
                    p_type,
                    p_registration_date_utc,
                    p_registration_date_utc,
                    1,
                    sys_extract_utc(systimestamp),
                    sys_extract_utc(systimestamp)
                );
        end;
    end register_device;

end easy_mobile_device;
/
```

## Classe Tools

Nom propose :

```csharp
Tools.EasyMobileDeviceConfig
```

Fichier propose :

```text
Tools/EasyMobileDeviceConfig.cs
```

Responsabilites :

- verifier que l'utilisateur est connecte avec `UserProfil.GetUser()`;
- parser le XML recu par le service SOAP;
- valider les champs obligatoires;
- calculer `push_token_hash` en SHA-256;
- recuperer `login`, `account_code` et `type` depuis la session;
- appeler `easy_mobile_device.register_device`;
- retourner un XML resultat.

Code :

```csharp
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Oracle.DataAccess.Client;

namespace Tools
{
    public static class EasyMobileDeviceConfig
    {
        public static string RegisterDevice(string registrationXml)
        {
            try
            {
                UserProfil user = UserProfil.GetUser();

                if (user == null || !user.IsUserLoggedIn)
                    return BuildResult(false, "Utilisateur non connecte.", null);

                if (string.IsNullOrWhiteSpace(registrationXml))
                    return BuildResult(false, "XML vide.", null);

                XElement root = XDocument.Parse(registrationXml).Root;

                if (root == null || root.Name.LocalName != "mobile_device_config")
                    return BuildResult(false, "Racine XML invalide.", null);

                string platform = GetXmlValue(root, "platform");
                string provider = GetXmlValue(root, "notification_provider");
                string pushToken = GetXmlValue(root, "push_token");

                if (string.IsNullOrWhiteSpace(platform) ||
                    string.IsNullOrWhiteSpace(provider) ||
                    string.IsNullOrWhiteSpace(pushToken))
                {
                    return BuildResult(false, "platform, notification_provider et push_token sont obligatoires.", null);
                }

                DateTime registrationDateUtc;
                if (!DateTime.TryParse(
                        GetXmlValue(root, "registration_date_utc"),
                        null,
                        DateTimeStyles.AdjustToUniversal,
                        out registrationDateUtc))
                {
                    registrationDateUtc = DateTime.UtcNow;
                }

                byte[] pushTokenHash = ComputeSha256(pushToken.Trim());

                if (user.EasyConnection.State != ConnectionState.Open)
                    user.EasyConnection.Open();

                using (DbCommand cmd = user.EasyConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "easy_mobile_device.register_device";

                    OracleCommand oracleCommand = cmd as OracleCommand;
                    if (oracleCommand != null)
                        oracleCommand.BindByName = true;

                    Add(cmd, "p_platform", platform);
                    Add(cmd, "p_notification_provider", provider);
                    Add(cmd, "p_push_token", pushToken.Trim());
                    Add(cmd, "p_push_token_hash", pushTokenHash);

                    Add(cmd, "p_installation_id", GetXmlValue(root, "installation_id"));
                    Add(cmd, "p_device_id", GetXmlValue(root, "device_id"));
                    Add(cmd, "p_application_id", GetXmlValue(root, "application_id"));
                    Add(cmd, "p_application_version", GetXmlValue(root, "application_version"));
                    Add(cmd, "p_device_model", GetXmlValue(root, "device_model"));
                    Add(cmd, "p_manufacturer", GetXmlValue(root, "manufacturer"));
                    Add(cmd, "p_operating_system_version", GetXmlValue(root, "operating_system_version"));

                    Add(cmd, "p_login", user.Login);
                    Add(cmd, "p_account_code", user.AccountCode);
                    Add(cmd, "p_type", user.Type);
                    Add(cmd, "p_registration_date_utc", registrationDateUtc);

                    DbParameter output = cmd.CreateParameter();
                    output.ParameterName = "p_mobile_device_config_id";
                    output.Direction = ParameterDirection.Output;
                    output.DbType = DbType.Binary;
                    output.Size = 16;
                    cmd.Parameters.Add(output);

                    cmd.ExecuteNonQuery();

                    string id = output.Value == null || output.Value == DBNull.Value
                        ? null
                        : BitConverter.ToString((byte[])output.Value).Replace("-", "");

                    return BuildResult(true, "Device configuration saved.", id);
                }
            }
            catch (Exception ex)
            {
                return BuildResult(false, ex.Message, null);
            }
        }

        private static string GetXmlValue(XElement root, string name)
        {
            XElement element = root.Element(name);
            return element == null ? null : (element.Value ?? string.Empty).Trim();
        }

        private static void Add(DbCommand cmd, string name, object value)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(parameter);
        }

        private static byte[] ComputeSha256(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string BuildResult(bool success, string message, string id)
        {
            XDocument doc = new XDocument(
                new XElement("mobile_device_config_result",
                    new XElement("success", success ? "1" : "0"),
                    new XElement("message", message ?? string.Empty),
                    new XElement("mobile_device_config_id", id ?? string.Empty)
                )
            );

            return doc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
```

## Fonction SOAP

Dans `ShoppingCartController.asmx.cs`, la fonction SOAP doit rester minimale :

```csharp
[WebMethod(EnableSession = true)]
public string RegisterMobileNotificationDevice(string registrationXml)
{
    return Tools.EasyMobileDeviceConfig.RegisterDevice(registrationXml);
}
```

Cette methode est authentifiee par la session ASP.NET : elle s'appuie sur `UserProfil.GetUser()` dans la classe `Tools.EasyMobileDeviceConfig`.

## XML attendu depuis l'application

```xml
<mobile_device_config>
  <application_id>fr.groupeagra.easymobile</application_id>
  <application_version>0.97</application_version>
  <platform>Android</platform>
  <notification_provider>FCM</notification_provider>
  <push_token>...</push_token>
  <installation_id>...</installation_id>
  <device_id>...</device_id>
  <device_model>...</device_model>
  <manufacturer>...</manufacturer>
  <operating_system_version>...</operating_system_version>
  <registration_date_utc>2026-07-14T10:30:00Z</registration_date_utc>
</mobile_device_config>
```

Pour iOS :

```xml
<platform>iOS</platform>
<notification_provider>APNS</notification_provider>
```

ou `FCM` si Firebase est utilise comme couche commune pour Android et iOS.

## XML resultat

```xml
<mobile_device_config_result>
  <success>1</success>
  <message>Device configuration saved.</message>
  <mobile_device_config_id>...</mobile_device_config_id>
</mobile_device_config_result>
```

En cas d'erreur :

```xml
<mobile_device_config_result>
  <success>0</success>
  <message>...</message>
  <mobile_device_config_id></mobile_device_config_id>
</mobile_device_config_result>
```

## Points d'attention

- Le token de push peut changer. L'application devra rappeler cette registration quand le token change.
- La table stocke le dernier etat connu pour un token donne.
- Si un meme token revient avec une date plus ancienne, la ligne existante n'est pas remplacee.
- L'unicite globale sur `push_token_hash` permet de synchroniser les plateformes sans dupliquer le meme token.
- `login`, `account_code` et `type` sont volontairement recuperes cote serveur pour eviter qu'un client mobile puisse mentir dans le XML.
- Le filtrage des notifications vers les clients se fera plus tard via `type = 'Client'`.
- Les noms d'objets Oracle ont ete raccourcis pour eviter les limites anciennes de nommage.
