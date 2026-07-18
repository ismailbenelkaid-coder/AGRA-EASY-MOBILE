# Conscience de passation - Notifications Firebase / EASY ASPX

Ce document est destine au projet serveur EASY ASP.NET / ASPX. Il explique ce qui a ete prepare cote Firebase et ce qu'il reste a faire cote serveur pour envoyer des notifications push vers l'application mobile AGRA EASY MOBILE.

## Objectif

Mettre en place l'envoi de notifications push compatibles Android et iOS pour l'application mobile MAUI AGRA EASY MOBILE.

Le mobile devra enregistrer son terminal via le service SOAP existant. Le serveur EASY devra ensuite utiliser Firebase Cloud Messaging pour envoyer les notifications vers les tokens enregistres.

## Dossier local contenant les fichiers Firebase

Tous les fichiers sensibles et les documents de configuration Firebase ont ete places dans le dossier local suivant du projet mobile :

```text
C:\Users\ism_b\Documents\Github\AGRA-EASY-MOBILE\Firebase\
```

Ce dossier est exclu de Git via `.gitignore`. Il ne doit pas etre commite.

Le dossier contient notamment :

```text
google-services.json
GoogleService-Info.plist
agra-easy-mobile-firebase-adminsdk-fbsvc-f03dc75112.json
AuthKey_8RGS4K7FL6.p8
AGRA EASY MOBILE - Parametres du projet - Parametres generaux - Console Firebase.pdf
AGRA EASY MOBILE - Parametres du projet - Parametres generaux - Console Firebase2.pdf
Certificates, Identifiers & Profiles - Apple Developer.pdf
```

Les numeros de projet Firebase, IDs d'application Android/iOS et informations visibles de configuration sont conserves dans les PDF presents dans ce dossier.

## Fichiers et roles

### google-services.json

Configuration Firebase Android. Ce fichier sert a l'application Android pour connaitre le projet Firebase.

Il concerne principalement le projet mobile, pas le serveur EASY.

### GoogleService-Info.plist

Configuration Firebase iOS. Ce fichier sert a l'application iOS pour connaitre le projet Firebase.

Il concerne principalement le projet mobile, pas le serveur EASY.

### AuthKey_8RGS4K7FL6.p8

Cle APNs Apple utilisee par Firebase pour envoyer les notifications vers iOS.

Cette cle a ete creee dans Apple Developer avec le service :

```text
Apple Push Notifications service (APNs)
```

Elle doit etre importee dans Firebase, section Cloud Messaging, pour l'application iOS.

Informations associees :

```text
Key ID : 8RGS4K7FL6
Team ID : L4MMR7BJ4T
```

Le fichier `.p8` ne doit jamais etre commite.

### agra-easy-mobile-firebase-adminsdk-fbsvc-f03dc75112.json

Cle de compte de service Firebase Admin.

C'est le fichier le plus important pour le serveur EASY : il permet au serveur ASP.NET d'obtenir un token OAuth Google puis d'appeler Firebase Cloud Messaging HTTP v1 pour envoyer une notification.

Ce fichier ne doit jamais etre commite. Il doit etre stocke cote serveur dans un emplacement securise, hors depot Git.

## Travail deja defini cote SOAP

Une methode SOAP doit exister ou etre ajoutee :

```csharp
RegisterMobileNotificationDevice(string registrationXml)
```

Elle doit etre authentifiee : l'utilisateur doit etre connecte.

La methode SOAP ne doit pas contenir la logique metier complete. Elle doit uniquement rediriger le XML recu vers une classe de traitement dans `Tools`, par exemple :

```csharp
Tools.EasyMobileDeviceConfig.RegisterDevice(registrationXml);
```

La classe `Tools.EasyMobileDeviceConfig` doit recuperer l'utilisateur connecte via :

```csharp
UserProfil.GetUser()
```

Il ne faut donc pas envoyer depuis le mobile :

```text
login
account_code
type
```

Ces champs doivent etre determines cote serveur a partir de la session utilisateur.

## XML attendu depuis le mobile

Le mobile doit transmettre un XML de configuration terminal ressemblant a ceci :

```xml
<mobile_device_config>
  <platform>android</platform>
  <push_provider>firebase</push_provider>
  <push_token>...</push_token>
  <device_id>...</device_id>
  <device_name>...</device_name>
  <device_model>...</device_model>
  <os_version>...</os_version>
  <app_version>...</app_version>
  <app_build>...</app_build>
  <locale>fr-FR</locale>
  <timezone>Europe/Paris</timezone>
  <registered_at_utc>2026-07-17T17:30:00Z</registered_at_utc>
</mobile_device_config>
```

Les noms de champs sont en minuscules avec underscore quand necessaire.

## Table Oracle cible

La table cible retenue est :

```text
easy_mobile_device_config
```

Regle importante :

```text
Le token push doit etre unique globalement, sans inclure la plateforme dans la cle unique.
```

Raison : le contenu de cette table pourra etre synchronise avec d'autres plateformes. Il faut donc eviter une unicite limitee a un couple token/plateforme.

Champs fonctionnels importants a prevoir :

```text
id
push_token_hash
push_token
platform
push_provider
device_id
device_name
device_model
os_version
app_version
app_build
locale
timezone
login
account_code
type
is_active
registered_at_utc
last_seen_at_utc
created_at
updated_at
```

Les contraintes Oracle doivent avoir des noms courts, compatibles Oracle 11g.

Exemples de noms courts :

```text
pk_easy_mobile_dev_cfg
ux_easy_mobile_dev_token
ck_easy_mobile_dev_active
```

## Package Oracle

Le package Oracle retenu doit s'appeler :

```text
easy_mobile_device
```

Il doit contenir la logique d'insertion/mise a jour de la table `easy_mobile_device_config`.

Comportement attendu :

1. Calculer ou recevoir `push_token_hash`.
2. Chercher si un terminal existe deja pour ce token.
3. Si le token existe, mettre a jour la ligne uniquement si l'information recue est plus recente ou equivalente.
4. Si le token n'existe pas, inserer une nouvelle ligne.
5. Conserver `login`, `account_code` et `type` recuperes depuis la session EASY.
6. Ne pas exclure les utilisateurs non Client a l'enregistrement.

Important :

```text
Tous les profils peuvent enregistrer leur terminal.
Le filtrage type = 'Client' ne doit etre fait que plus tard, au moment de choisir les destinataires d'une notification.
```

## Envoi de notifications depuis le serveur EASY

Pour envoyer une notification, le serveur devra utiliser Firebase Cloud Messaging HTTP v1.

Principe :

1. Lire la cle serveur JSON Firebase Admin depuis un emplacement securise.
2. Obtenir un access token OAuth Google avec le scope FCM.
3. Appeler l'endpoint FCM HTTP v1.
4. Envoyer le message au `push_token` cible.
5. Traiter les erreurs Firebase pour desactiver les tokens invalides.

Endpoint FCM HTTP v1 :

```text
https://fcm.googleapis.com/v1/projects/{project_id}/messages:send
```

Le `{project_id}` est visible dans les PDF Firebase du dossier `Firebase`.

Attention au serveur ASP.NET 4.5 :

```csharp
System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
```

Cette ligne peut etre necessaire avant les appels HTTPS vers Google/Firebase.

## Pieges a eviter

Ne jamais commiter :

```text
Firebase/*.json
Firebase/*.p8
apple-signing-local/*
*.p12
*.mobileprovision
*.cer
*.jks
*.keystore
```

Ne pas confondre :

```text
APNs .p8
```

avec :

```text
certificat de signature iOS .p12 / .mobileprovision
```

La cle APNs sert aux notifications iOS via Firebase.
Le certificat de signature sert a compiler et publier l'application iOS.

Ne pas utiliser la cle serveur Firebase dans l'application mobile.
Elle doit rester uniquement cote serveur.

Ne pas filtrer les profils non Client au moment de l'enregistrement du terminal.
Le filtrage des destinataires se fera au moment de l'envoi des notifications.

## Prochaine etape conseillee

1. Copier la cle serveur Firebase Admin JSON dans un emplacement securise du serveur EASY.
2. Ajouter un parametre de configuration serveur indiquant son chemin.
3. Implementer ou finaliser `Tools.EasyMobileDeviceConfig`.
4. Implementer ou finaliser le package Oracle `easy_mobile_device`.
5. Ajouter une fonction serveur d'envoi FCM HTTP v1.
6. Faire un premier test avec un token Android, puis iOS.

