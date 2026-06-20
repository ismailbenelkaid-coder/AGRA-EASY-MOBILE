# Suivi AGRA-EASY-MOBILE - demandes et versions

## Version47 - base Version46-Update1

Base de travail prise en compte : `AGRA-EASY-MOBILE-Version46-Update1.zip`, fourni avec références de services web mises à jour.

Corrections appliquées dans Version47 :

1. Services panier commande
   - Remplacement des usages mobiles de `GetShoppingCartAccountCode` par `GetOrderBasketAccountCode`.
   - Remplacement des usages mobiles de `SetShoppingCartAccountCode` par `SetOrderBasketAccountCode`.
   - Ajout des wrappers correspondants dans `EasySession.cs`, copiés sur le style existant `ExecuteWithRetryAsync`.
   - Aucun changement de `Reference.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou logique de connexion/reconnexion.

2. Classe client
   - Remplacement de l'usage de `ReturnBasketAccount` par `ClientAccount` dans la gestion de déclaration de retour, conformément à la nouvelle référence de services web.
   - Utilisation des propriétés disponibles `AccountCode`, `AccountName`, `Zip`, `City`, `AddressLine1`, `AddressLine2`.

3. Vue Catalogue - sélection client panier commande
   - Le bouton du code client est renommé/traité comme un bouton de choix client.
   - Le bouton n'affecte plus directement une saisie brute.
   - Ouverture d'une vue modale de recherche client basée sur `FindClientAccount(Keyword)`.
   - En-tête fixe avec zone de filtre.
   - Recherche déclenchée uniquement en fin de saisie (`Completed` / `Unfocused`) dans la zone filtre, pas à chaque caractère.
   - Liste centrale scrollable de cartes client.
   - Chaque carte affiche le code, le nom, puis `Code postal - Ville` si disponible.
   - Pied fixe avec deux boutons sur la même ligne : validation du client sélectionné et `Annuler`.
   - Le bouton de validation est visible uniquement si une liste est retournée et qu'un client est sélectionné.
   - La validation transfère le code client choisi dans la zone code client et applique l'affectation au panier.
   - Changement de client interdit si le panier commande n'est pas vide.

4. Vue gestion déclaration de retour
   - Même vue de recherche/sélection client que le Catalogue.
   - Même service `FindClientAccount(Keyword)`.
   - Même déclenchement en fin de saisie du filtre.
   - Même présentation en cartes : code, nom, code postal - ville.
   - Même pied fixe `Valider` / `Annuler`.
   - Changement de client interdit si le panier de retour n'est pas vide.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Règles permanentes rappelées

- Ne jamais générer physiquement une nouvelle version/ZIP sans demande explicite.
- Ne pas improviser de logique métier, service web, fallback ou formatage non demandé.
- Ne pas modifier `Reference.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ni la logique de connexion/reconnexion sans autorisation explicite.
- Les valeurs métier envoyées par le service web doivent être affichées telles quelles, sans reformatage culturel non demandé.
- Les champs de saisie doivent être encadrés, sans trait noir natif, avec saisie centrée horizontalement et verticalement.
- Chaque ZIP généré doit inclure ce suivi et le document Word de transfert/CDC à jour.

## Version48 - base Version47

Base de travail : `AGRA-EASY-MOBILE-Version47.zip`.

Corrections appliquees dans Version48 :

1. Filtres Expedition et Retour - normalisation automatique du code article
   - Dans les vues d'edition des filtres Expedition et Retour, la fin de saisie de la reference article declenche une recherche via le service web `FindProductCodeList`.
   - Le declenchement se fait uniquement en fin de saisie (`Completed` / `Unfocused`), jamais a chaque caractere.
   - Si la saisie est vide, aucun appel service web n'est effectue.
   - Si la liste retournee n'est pas vide, la saisie est remplacee par le premier `ProductCode` retourne.
   - Une protection anti-double appel evite de relancer la meme recherche lorsque `Completed` et `Unfocused` se suivent.

2. Selection article assistee dans les filtres
   - Ajout d'un bouton `Choisir` a cote de la zone de saisie article dans `ExpeditionFilterView` et `ReturnFilterView`.
   - Ajout d'une vue commune `ProductCodeSelectionPage.cs`, sur le meme principe que `ClientAccountSelectionPage`.
   - La vue contient un en-tete fixe avec filtre, une liste centrale scrollable de cartes article et un pied fixe avec `Valider` / `Annuler` sur une ligne partagee.
   - L'appel `FindProductCodeList` est lance uniquement en fin de saisie du filtre.
   - Les cartes affichent uniquement des proprietes existantes de `ReturnableArticle` : `ProductCode`, `ProductLabel`, `SupplierName`.
   - La validation transfere le `ProductCode` selectionne dans la zone article de la vue appelante.

3. Service web
   - Ajout du wrapper `FindProductCodeListAsync(string keyword, bool onlyActiveProduct, bool isGenCode)` dans `EasySession.cs`, copie sur le style existant `ExecuteWithRetryAsync`.
   - Aucun changement de `Reference.cs`, de `ExecuteWithRetryAsync`, de `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Les appels effectues par les filtres utilisent `FindProductCodeListAsync(keyword, false, false)` afin de ne pas filtrer arbitrairement les articles actifs et de traiter la saisie comme un code article, pas comme un code generique.

Limites : compilation reelle non executee dans l'environnement car `dotnet` n'est pas disponible.


## Version49 - base Version48

Base de travail : `AGRA-EASY-MOBILE-Version48.zip`.

Corrections appliquées dans Version49 :

1. Bouton de sélection article dans les filtres Expédition et Retour
   - Remplacement du gros bouton texte `Choisir` par un petit bouton icône, cohérent avec les petits boutons du projet.
   - Objectif : laisser plus d'espace au champ de référence article.
   - Aucun changement du service web utilisé ni de la logique de sélection.

2. Transmission de dates vides au service web par saisie contrôlée
   - Pas de case à cocher ajoutée.
   - Les deux dates peuvent être toutes les deux renseignées ou toutes les deux laissées vides.
   - Si une seule date est renseignée, un message bloquant informe l'utilisateur.
   - Si les deux dates sont vides, `FirstDate` et `LastDate` sont enregistrées à `null` et transmises ainsi au service web ; aucune logique métier locale de désactivation n'est ajoutée.
   - La saisie directe des dates est contrôlée au format `jj/mm/aaaa`, avec acceptation des formats `dd/MM/yyyy` et `d/M/yyyy`.

3. Sélection article avec dernière saisie utilisateur
   - Avant toute correction automatique par le premier `ProductCode` retourné par `FindProductCodeList`, la dernière saisie brute de l'utilisateur est conservée.
   - Lors de l'ouverture de la vue de sélection article, le filtre initial utilise cette dernière saisie utilisateur, et non le code corrigé automatiquement.
   - Exemple : si l'utilisateur saisit `LS923` et que le champ est corrigé en `415280...`, la vue de sélection s'ouvre avec `LS923`.

4. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Aucun changement de formatage des valeurs métier.
   - Aucune modification hors périmètre demandé.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version50 - base Version49

Base de travail : `AGRA-EASY-MOBILE-Version49.zip`.

Corrections appliquées dans Version50 :

1. Filtres Expédition et Retour - dates contrôlées et assistées
   - Les zones de date restent des champs de saisie manuelle encadrés.
   - La saisie est assistée : les `/` sont insérés automatiquement au format `jj/mm/aaaa`.
   - Les champs limitent la saisie utile à 8 chiffres, affichés sous forme `dd/MM/yyyy`.
   - Si l'utilisateur quitte un champ avec une date incomplète ou invalide, un message bloquant l'informe et le focus revient sur le champ.
   - La règle déjà validée est conservée : les deux dates doivent être soit toutes les deux renseignées, soit toutes les deux vides.
   - Si les deux dates sont vides, l'application transmet simplement `FirstDate = null` et `LastDate = null` au service web ; aucune logique métier locale supplémentaire n'est ajoutée.
   - Ajout du contrôle bloquant : la date de fin ne peut pas être antérieure à la date de début.

2. Filtres Expédition et Retour - bouton calendrier
   - Ajout d'un petit bouton icône calendrier dans chaque zone de date.
   - Le bouton ouvre le calendrier natif et remplit le champ date ciblé.
   - Le design reste cohérent avec les petits boutons icônes ajoutés pour la sélection article.

3. Filtres Expédition et Retour - aide à la saisie du code client
   - Ajout d'un petit bouton icône dans la zone `Code client`, avec le même esprit que le bouton d'aide à la saisie article.
   - Le bouton ouvre la vue commune `ClientAccountSelectionPage` alimentée par `FindClientAccount`.
   - Le comportement existant de la zone `Code client` n'est pas modifié.
   - Le bouton suit la visibilité et l'activation du champ : si le champ est masqué ou désactivé selon les règles actuelles, l'aide à la sélection client ne permet pas de contourner cette règle.
   - La validation transfère uniquement le `AccountCode` sélectionné dans le champ `Code client` de la vue appelante.

4. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Aucun changement de services web, de logique métier des filtres, ni de formatage des valeurs métier.
   - Les contrôles ajoutés sont limités aux vues `ExpeditionFilterView` et `ReturnFilterView`.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.


## Version51 - base Version50

Base de travail : `AGRA-EASY-MOBILE-Version50.zip`.

Corrections appliquées dans Version51 :

1. Filtres Expédition et Retour - saisie date réellement assistée
   - Correction de la saisie manuelle des dates : les `/` sont insérés automatiquement en cours de saisie sans repositionner systématiquement le curseur à la fin.
   - La position du curseur est recalculée à partir du nombre de chiffres avant le curseur, afin de permettre la modification du jour, du mois ou de l'année au milieu du champ.
   - La règle existante est conservée : deux dates renseignées ou deux dates vides ; une date seule déclenche un message bloquant ; une date invalide ou incomplète déclenche un message bloquant en fin de saisie.
   - Le contrôle date de fin >= date de début est conservé.
   - Si les deux dates sont vides, l'application transmet simplement `FirstDate = null` et `LastDate = null` au service web.

2. Filtres Expédition et Retour - calendrier
   - Correction du bouton icône calendrier : il ouvre désormais une petite vue modale avec un `DatePicker` visible et utilisable, puis remplit la zone date au format `dd/MM/yyyy` après validation.
   - Le calendrier n'est plus déclenché via un `DatePicker` masqué ou trop petit, ce qui évite le non-fonctionnement constaté après Version50.

3. Fenêtres de recherche client et article
   - Dans `ClientAccountSelectionPage` et `ProductCodeSelectionPage`, la zone de filtre est centrée horizontalement et verticalement.
   - Les boutons de pied sont inversés pour respecter le reste de l'application : `Annuler` à gauche et `Valider` à droite.

4. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Aucun changement de services web, de logique métier des filtres, ni de formatage des valeurs métier.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version52 - base Version51

Base de travail : `AGRA-EASY-MOBILE-Version51.zip`.

Corrections appliquées dans Version52 :

1. Filtres Expédition et Retour - saisie date libre
   - Abandon de la saisie assistée/masquée avec insertion automatique des `/`.
   - Les zones de date restent des champs de saisie libres, encadrés et centrés.
   - Aucun contrôle bloquant n'est exécuté à la sortie du champ date ou pendant la saisie.
   - Le contrôle des dates est réalisé uniquement lors du clic sur `Valider` dans la vue d'édition du filtre.
   - La règle existante est conservée : les deux dates doivent être soit toutes les deux renseignées, soit toutes les deux vides.
   - Si les deux dates sont vides, `FirstDate = null` et `LastDate = null` sont simplement transmis au service web.
   - Si les deux dates sont renseignées, leur validité est contrôlée au format `dd/MM/yyyy` ou `d/M/yyyy`.
   - La date de fin doit être supérieure ou égale à la date de début.
   - En cas d'erreur, l'utilisateur reste dans la vue et peut uniquement corriger ou utiliser le bouton `Annuler`.

2. Filtres Expédition et Retour - calendrier direct
   - Suppression de la vue modale intermédiaire ajoutée en Version51 pour le calendrier.
   - Le bouton icône calendrier utilise un `DatePicker` attaché directement à la vue de filtre.
   - Le clic sur l'icône ouvre directement le calendrier natif autant que possible par MAUI, puis remplit le champ date ciblé au format `dd/MM/yyyy` après sélection.

3. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Aucun changement de services web, de logique métier hors validation demandée, ni de formatage des valeurs métier.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version53 - base Version52

Base de travail : `AGRA-EASY-MOBILE-Version52.zip`.

Corrections appliquées dans Version53 :

1. Filtres Expédition et Retour - ouverture directe du calendrier
   - Correction du bouton calendrier qui ne lançait pas le sélecteur natif dans Version52.
   - Suppression du montage fragile `ImageButton` + `picker.Focus()` : l'icône n'intercepte plus le clic à la place du `DatePicker`.
   - Le `DatePicker` est désormais le contrôle réellement cliquable dans la zone de l'icône calendrier, avec une icône visuelle en arrière-plan.
   - Le clic sur la zone calendrier ouvre directement le calendrier natif MAUI autant que possible sur Android et Windows.
   - Après sélection, le champ date est rempli au format `dd/MM/yyyy`.

2. Comportement conservé
   - La saisie date reste libre.
   - Aucun contrôle bloquant n'est fait pendant la saisie ni à la sortie du champ.
   - Le contrôle des dates reste uniquement au clic sur `Valider` : deux dates renseignées ou deux dates vides, format valide si renseigné, et date de fin supérieure ou égale à la date de début.
   - Si les deux dates sont vides, `FirstDate = null` et `LastDate = null` sont transmis simplement au service web.

3. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.
   - Aucun changement de services web, de logique métier hors correction du calendrier, ni de formatage des valeurs métier.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.


## Version54 - base Version53

Base de travail : `AGRA-EASY-MOBILE-Version53.zip`.

Corrections appliquées dans Version54 :

1. Vue Catalogue - nouveau mode de recherche par immatriculation
   - Ajout d'un bouton icône à droite de la zone de recherche pour basculer entre les modes `Référence` et `Immatriculation`.
   - Le mode `Référence` conserve le fonctionnement existant sans modification.
   - Le mode `Immatriculation` est réservé aux utilisateurs administrateurs.
   - Le placeholder de la zone de recherche devient `Référence` ou `Immatriculation` selon le mode actif.
   - La recherche par immatriculation appelle `GetVehiculeFromImmatriculation`, puis charge l'arborescence TecDoc via `GetKTypeAlternative`.
   - Ajout d'une sous-vue interne de véhicule identifié et de navigation dans les familles de pièces, sans modifier les cartes article existantes.
   - Les noeuds de famille utilisent `IconPath` ou `IconName` quand l'information est disponible, avec un affichage de secours sinon.
   - Dès qu'une famille retourne une liste d'articles, le flux rejoint l'affichage existant des groupes d'articles (`Articles et conditions de vente`, `Article stocké ou géré en stock`, `Autres articles`, etc.).
   - Les options existantes comme `Afficher tous les articles`, `Afficher les images` et la sélection des plateformes restent appliquées sans changement de logique métier.

2. Filtres Expédition et Retour - calendrier
   - Lorsqu'un calendrier est ouvert via le bouton icône, la valeur déjà saisie dans le champ date est relue.
   - Si la valeur est une date valide au format `dd/MM/yyyy` ou `d/M/yyyy`, le `DatePicker` est initialisé avec cette date.
   - Si la valeur est vide ou invalide, le comportement existant est conservé sans blocage.

3. Services / wrappers
   - Ajout des wrappers `GetVehiculeFromImmatriculationAsync` et `GetKTypeAlternativeAsync` dans `EasySession.cs`, en respectant le style `ExecuteWithRetryAsync`.
   - Aucun changement de `Reference.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.

4. Contraintes respectées
   - Pas de modification du mode de recherche par référence existant.
   - Pas de modification des cartes article déjà validées.
   - Pas de modification des valeurs métier ni de formatage.
   - Pas de modification des services web existants hors ajout de wrappers nécessaires.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version55 - base Version54

Base de travail : `AGRA-EASY-MOBILE-Version54.zip`.

Corrections appliquées dans Version55 :

1. Recherche par immatriculation - visibilité du bouton de mode
   - Le bouton icône permettant de basculer entre les modes `Référence` et `Immatriculation` est visible uniquement pour les utilisateurs de type Administrateur.
   - Pour les utilisateurs de type Client, le bouton est retiré de la ligne de recherche et la zone de saisie reprend toute la largeur disponible jusqu'au bouton `Chercher`, sans espace vide intermédiaire.
   - Le mode Client reste forcé en recherche par référence.

2. Recherche par immatriculation - retour dans l'arborescence TecDoc
   - La commande `Revenir` ne restitue pas une vue gardée en mémoire.
   - En mode immatriculation, lorsqu'une navigation TecDoc est en cours, `Revenir` relance un appel `GetKTypeAlternativeAsync` avec le KType et le nœud parent approprié pour recharger le niveau précédent.
   - Cette logique s'applique pendant la navigation dans l'arborescence et aussi lorsque les articles finaux sont affichés.

3. Recherche par immatriculation - navigation visible avec les articles
   - Quand des articles sont affichés depuis une famille TecDoc, le bloc véhicule/navigation/familles reste visible au-dessus des groupes d'articles existants.
   - Les cartes article et leur fonctionnement validé restent inchangés.

4. Recherche par immatriculation - icône de secours des familles
   - L'icône de secours utilisée lorsqu'un nœud TecDoc n'a pas `IconPath` ou `IconName` a été remplacée par un symbole de catégorie/pièce plus neutre et explicite.
   - L'objectif est d'éviter l'ambiguïté avec une icône qui laisserait croire à des sous-arbres.

5. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync`, logique de connexion/reconnexion, services web ou formatage des valeurs métier.
   - Les fichiers de suivi ont été mis à jour et inclus dans le ZIP.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version56 - base Version55

Base de travail : `AGRA-EASY-MOBILE-Version55.zip`.

Corrections appliquées dans Version56 :

1. Compilation CatalogueView
   - Correction de l'erreur CS0266 dans `Views/CatalogueView.cs` : le helper `EditBox(Entry entry)` retourne désormais explicitement un `Border`, car il construit toujours un `new Border`.
   - Cette correction supprime l'affectation impossible d'un `View` vers `_searchEntryBox` déclaré en `Border?`.

2. Recherche par immatriculation - fil d'arborescence TecDoc
   - Observation confirmée : le nœud/famille sélectionné n'était pas ajouté immédiatement dans le fil d'arborescence parcouru ; il pouvait n'apparaître qu'après sélection d'un enfant.
   - Correction : dès qu'une famille est sélectionnée, elle est intégrée au chemin de navigation cible utilisé pour reconstruire la vue après l'appel service web.
   - Le fil représente maintenant le nœud actuellement consulté, y compris quand ce nœud retourne directement des articles.
   - Les retours de niveau continuent d'appeler `GetKTypeAlternativeAsync` avec le KType et le nœud parent approprié, sans réafficher des familles gardées en mémoire.

3. Filtres Expédition / Suivi commandes et Retour - bouton Annuler
   - Si un administrateur arrive depuis le menu sur `OrderListView` ou `ReturnListView` sans filtre valide, la vue filtre s'ouvre automatiquement.
   - Au clic sur `Annuler`, si le filtre correspondant reste invalide, la modale est fermée puis la navigation revient directement à `//home`.
   - Cela évite que `OnAppearing()` de la liste rouvre immédiatement la vue filtre et donne l'impression qu'Annuler ne fonctionne pas.
   - Les règles métier de validation des filtres restent inchangées.

4. Filtres Expédition / Retour - synchronisation du calendrier
   - Le `DatePicker` est synchronisé silencieusement avec le champ date dès que le texte saisi correspond à une date valide (`dd/MM/yyyy` ou `d/M/yyyy`).
   - La synchronisation est aussi faite au chargement initial du filtre et au focus du `DatePicker`.
   - Le calendrier reprend donc la date déjà saisie quand elle est valide.
   - La saisie reste libre ; aucun contrôle bloquant n'est exécuté pendant la saisie. Les contrôles restent au clic sur `Valider`.

5. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync`, logique de connexion/reconnexion, services web, cartes article existantes ou formatage des valeurs métier.
   - Les fichiers de suivi ont été mis à jour et inclus dans le ZIP.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version57 - base Version56

Base de travail : `AGRA-EASY-MOBILE-Version56.zip`.

Correction appliquée dans Version57 :

1. Correction compilation filtres Expédition / Retour
   - Correction des erreurs CS1061 dans `Views/ExpeditionFilterView.xaml.cs` et `Views/ReturnFilterView.xaml.cs`.
   - Cause : la synchronisation du calendrier comparait `picker.Date.Date` alors que, dans l'environnement cible .NET/MAUI net10, `DatePicker.Date` peut être exposé comme `DateTime?` ; l'appel direct à `.Date` sur un nullable provoque l'erreur.
   - Correction retenue : comparer directement `picker.Date` avec `parsedDate.Date`, puis affecter `picker.Date = parsedDate.Date`.
   - Cette correction conserve le comportement attendu : lorsque le champ date contient une date valide, le calendrier reprend cette date avant ouverture.
   - Aucune suppression fonctionnelle ni contournement par retrait de la logique de synchronisation.

2. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync`, logique de connexion/reconnexion, services web, cartes article existantes ou formatage des valeurs métier.
   - Correction strictement limitée aux deux erreurs de compilation signalées.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.


## Version58 - base Version57

Base de travail : `AGRA-EASY-MOBILE-Version57.zip`.

Correction appliquée dans Version58 :

1. Recherche par immatriculation - dernier nœud du fil TecDoc non cliquable
   - Dans la liste de navigation/arborescence TecDoc, le dernier nœud du chemin correspond au contenu actuellement affiché en dessous.
   - Ce dernier nœud n'est plus cliquable : il sert de légende/titre de contexte pour les sous-familles ou les articles affichés ensuite.
   - Les nœuds précédents restent cliquables pour remonter dans l'arborescence, avec appel service web `GetKTypeAlternativeAsync` comme déjà validé.
   - Le comportement du mode référence, les cartes article existantes et la logique métier ne sont pas modifiés.

2. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync`, logique de connexion/reconnexion, services web, cartes article existantes ou formatage des valeurs métier.
   - Correction strictement limitée à l'affichage et à la navigation du dernier nœud du fil TecDoc en mode immatriculation.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version59 - base Version58

Base de travail : `AGRA-EASY-MOBILE-Version58.zip`.

Correction appliquée dans Version59 :

1. Recherche par immatriculation - masquage des sections vides
   - Dans la sous-vue TecDoc de la vue Catalogue, la section `Familles de pièces` n'est plus affichée lorsque le service web ne retourne aucune sous-famille / famille enfant.
   - Le message `Aucune sous-famille disponible.` n'est plus affiché dans ce cas : la section est simplement absente.
   - La section `Articles et conditions de vente` n'est plus affichée lorsque le résultat article ne contient aucun groupe/carte article à afficher.
   - En mode immatriculation, si aucune famille et aucun article ne sont disponibles, la vue conserve seulement les éléments de contexte utiles déjà présents, notamment véhicule et navigation.

2. Contraintes respectées
   - Aucun changement de `Reference.cs`, `EasySession.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync`, logique de connexion/reconnexion, services web, cartes article existantes ou formatage des valeurs métier.
   - Correction strictement limitée à l'affichage conditionnel des sections vides.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## État de transfert après Version59

- Dernière version générée : `AGRA-EASY-MOBILE-Version59.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version58.zip`.
- Le mode de recherche par immatriculation existe dans la vue Catalogue, réservé administrateur, avec navigation TecDoc, retour par appel service web, dernier nœud non cliquable et sections vides masquées.
- Le mode de recherche par référence doit rester inchangé.
- Aucune demande de correction mobile n'est en attente après Version59, sauf nouvelle demande explicite de l'utilisateur.
- Ne jamais générer de ZIP sans demande explicite.

## Version60 - base Version59-Update1

Base de travail : `AGRA-EASY-MOBILE-Version59-Update1.zip`.

Corrections appliquées dans Version60 :

1. Contrôle technique de session service web
   - Remplacement de l'appel technique `IsClientAccountAsync()` par `IsConnectedAsync()` dans `EasySession.TryRecoverConnectionAsync`.
   - `IsConnectedAsync()` est traité comme un simple contrôle oui/non de validité de session.
   - Si `IsConnectedAsync()` retourne `true`, la session est considérée encore valable et la logique existante ne tente pas de reconnexion.
   - Si `IsConnectedAsync()` retourne `false` ou lève une exception, la session est considérée non connectée et la logique existante tente une reconnexion.
   - Aucune autre utilisation de `IsClientAccount` n'a été trouvée hors de `Reference.cs`, qui reste généré et non modifié.
   - La détermination Client / Administrateur reste basée sur le retour de connexion (`CurrentAccount.Type`).

2. Suivi des retours refusés
   - Suppression de l'écran placeholder `DeniedReturnView` et remplacement par la nouvelle vue réelle `RefusedReturnListView`.
   - Le menu existant `deniedReturn` est conservé techniquement, mais son libellé visible devient `Retours refusés`.
   - La vue utilise les services web ajoutés dans Version59-Update1 : `GetRefusedReturnsLines` et `GetRefusedReturnLine`.
   - Ajout des wrappers correspondants dans `EasySession.cs`, sans modification de `Reference.cs`.
   - La vue réutilise impérativement `ReturnFilterView` ; aucun nouveau filtre spécifique aux retours refusés n'a été créé.
   - Quand `ReturnFilterView` est ouverte depuis `RefusedReturnListView`, seuls les champs `État de retour` et `Status de traitement` sont masqués, et leurs valeurs ne sont pas transmises aux services des retours refusés.
   - L'affichage est uniquement détaillé : pas de mode entête, pas de mode ligne, pas de case `Afficher détail`, pas de bouton `Lignes`.
   - Structure de carte appliquée : en-tête avec `RefusedReturnLineCode`, entrepôt centré et date de création ; première ligne avec `ReturnCode` et `ProductCode` ; deuxième ligne avec `ReturnClientCode` et `Quantity` ; troisième/quatrième lignes pour le motif `Reason` sur deux lignes maximum.
   - Les modes de tri disponibles sont : `Par défaut`, `Article`, `Code retour`, `Code retour client`, `Date création`.
   - Un bouton d'icône `...` affiche le code retour client complet quand il est tronqué.
   - À la fin de la zone motif, un bouton `...` est affiché uniquement lorsque le motif est incomplet/tronqué ; il récupère le détail via `GetRefusedReturnLine` puis affiche le motif complet dans un message bloquant.
   - Un bouton image est affiché quand `PicturePath` est renseigné ; il récupère le détail via `GetRefusedReturnLine` puis affiche l'image en plein écran avec un bouton flottant de fermeture.
   - `PicturePath` est utilisé tel que retourné par le service web, sans préfixe ni reconstruction de chemin.

3. Panier de commande
   - Dans la carte plateforme, la ligne `Message` n'est plus affichée lorsque `WarehouseStatus` est vide ; le texte `Message : -` n'est donc plus produit.
   - Dans la carte `Mon panier`, la ligne `Total commande` située après les boutons de validation est supprimée, car redondante avec les totaux déjà présents.
   - Dans la carte `Mon panier`, la ligne client est masquée pour les utilisateurs dont `CurrentAccount.Type == "Client"`, y compris les clients gestionnaires de commandes.
   - Les calculs, totaux, quantités et valeurs retournées par les services web ne sont pas recalculés ni modifiés.

4. Documentation et suivi
   - Les fichiers de suivi projet ont été mis à jour.
   - La documentation des services web par scénarios a été intégrée/mise à jour dans le projet afin d'inclure le scénario `Suivi des retours refusés`, les services `GetRefusedReturnsLines` / `GetRefusedReturnLine`, et la classe `RefusedReturnLine`.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## État de transfert après Version60

- Dernière version générée : `AGRA-EASY-MOBILE-Version60.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version59-Update1.zip`.
- Le suivi des retours refusés est désormais implémenté via `RefusedReturnListView` raccordée au menu `Retours refusés`.
- `DeniedReturnView` a été supprimée car elle n'était qu'un placeholder remplacé.
- Le contrôle technique de session utilise désormais `IsConnectedAsync()` et non plus `IsClientAccountAsync()`.
- Les corrections panier demandées pour message vide, total redondant et ligne client Client sont appliquées.
- La documentation service web par scénarios doit continuer à être maintenue à chaque nouvelle version impactant les scénarios ou services utilisés.
- Les corrections antérieures mises en discussion mais non incluses explicitement dans Version60 restent à valider séparément avant intégration.
- Ne jamais générer de ZIP sans demande explicite.


## Version61 - base Version60

Base de travail : `AGRA-EASY-MOBILE-Version60.zip`.

Corrections appliquées dans Version61 :

1. Retours refusés - encadrement du motif et boutons intégrés
   - Dans `RefusedReturnListView`, le motif `Reason` est désormais affiché dans une zone encadrée couvrant entièrement deux lignes.
   - Le texte du motif reste aligné à gauche et est limité à deux lignes, sans centrage forcé.
   - Une petite colonne à droite de l'encadrement est réservée aux actions, afin que le texte ne déborde pas sous les boutons.
   - Le bouton image est placé en haut à droite de l'encadrement lorsque `PicturePath` est disponible.
   - Le bouton motif complet est placé en bas à droite de l'encadrement et reste visible dès qu'un motif existe, même si le motif n'est pas tronqué.
   - Les boutons sont dimensionnés pour tenir à l'intérieur de la hauteur de deux lignes, sans agrandir inutilement la carte.
   - Les deux actions continuent de relire le détail via `GetRefusedReturnLine` avant affichage.
   - `PicturePath` reste utilisé tel que retourné par le service web, sans préfixe ni reconstruction de chemin.

2. Changement de login - reconstruction logique de l'application
   - `EasySession` mémorise désormais le login de la session courante dans `CurrentLogin` après connexion réussie.
   - Lors d'une nouvelle connexion, si le login ou l'identité métier retournée diffère de la session précédente, l'application marque la racine Shell comme devant être reconstruite.
   - Après connexion réussie, la racine de navigation est remplacée par une nouvelle instance de `AppShell` avant la navigation vers l'accueil.
   - Cette reconstruction évite de conserver en mémoire des vues déjà chargées avec les droits, zones visibles ou états visuels de l'ancien utilisateur.
   - Les filtres expédition et retour sont toujours purgés lors de ce changement.

3. Catalogue - exception sections vides en recherche immatriculation
   - Les règles de Version59 sont conservées : la section `Familles de pièces` reste masquée si aucune sous-famille n'est retournée, et la section `Articles et conditions de vente` reste masquée si aucun article n'est disponible.
   - Exception ajoutée : si les deux sections sont vides en même temps, la section `Articles et conditions de vente` est réaffichée avec le message `Aucun article à afficher.`.
   - L'objectif est d'éviter une vue TecDoc trop vide ou ambiguë quand aucun article et aucune famille ne sont disponibles.

4. Catalogue - fenêtre Options catalogue
   - La fenêtre `Options catalogue` est agrandie dynamiquement afin que les huit plateformes actuelles soient visibles par défaut sans scroll sur un écran standard.
   - Le scroll est conservé dans le contenu de la fenêtre pour supporter l'ajout futur de plateformes supplémentaires.
   - Les boutons `Annuler` et `Appliquer` restent dans une zone de bas de fenêtre séparée du contenu scrollable.

5. Documentation et suivi
   - Les fichiers de suivi projet ont été mis à jour.
   - Le document de transfert / CDC a été mis à jour avec les corrections Version61.
   - La documentation service web par scénarios a été actualisée, notamment pour remplacer la présentation de `IsClientAccount` par `IsConnected` dans le scénario technique de session et pour tenir compte du scénario retours refusés Version61.

Contraintes respectées :
- Aucun changement de `Reference.cs`.
- Aucun service web ou champ non présent dans la version fournie n'a été inventé.
- Les valeurs métier retournées par les services web ne sont pas recalculées.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## État de transfert après Version61

- Dernière version générée : `AGRA-EASY-MOBILE-Version61.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version60.zip`.
- Les trois demandes antérieures oubliées après Version60 sont intégrées : reconstruction logique si changement de login, exception catalogue familles/articles vides, et agrandissement de la fenêtre Options catalogue.
- Le suivi des retours refusés conserve `ReturnFilterView`, `GetRefusedReturnsLines` et `GetRefusedReturnLine` ; la zone motif est maintenant encadrée avec boutons intégrés.
- La documentation service web par scénarios doit continuer à être maintenue à chaque nouvelle version impactant les scénarios ou services utilisés.
- Ne jamais générer de ZIP sans demande explicite.

## Version62 - base Version61

Base de travail : `AGRA-EASY-MOBILE-Version61.zip`.

Corrections appliquées dans Version62 :

1. Retours refusés - encadrement motif et boutons à hauteur de ligne
   - Correction de la zone motif dans `RefusedReturnListView` : les actions ne sont plus des contrôles `Button` / `ImageButton` imposant une hauteur native trop grande.
   - Les deux actions sont intégrées sous forme de petits `Border` cliquables via `TapGestureRecognizer`, afin de respecter la hauteur prévue dans l'encadrement.
   - La colonne de droite de l'encadrement reste découpée en deux cellules correspondant aux deux lignes du motif : image en haut, motif complet en bas.
   - Le texte du motif reste aligné à gauche, limité à deux lignes, et ne déborde pas sous les boutons.
   - Le bouton image reste visible uniquement si `PicturePath` est renseigné ; le bouton motif complet reste visible dès qu'un motif existe.
   - Les actions relisent toujours le détail via `GetRefusedReturnLine` avant affichage, et `PicturePath` reste utilisé tel que retourné par le service web.

2. Catalogue - fin de chemin TecDoc en recherche immatriculation
   - Correction du cas où une famille TecDoc sélectionnée ne contient ni article, ni sous-famille enfant.
   - La navigation / le fil TecDoc reste visible pour permettre de revenir en arrière, mais il n'empêche plus l'affichage du message de fin de chemin.
   - Une section `Articles et conditions de vente` est affichée avec le message `Aucun article à sélectionner.` lorsque la famille courante ne permet plus d'avancer et ne retourne aucun article.
   - Cette règle regarde le contenu disponible dans la famille courante, et non les familles présentes aux niveaux précédents de l'arborescence.

3. Masquage des zones client pour utilisateurs de type Client
   - Dans `CatalogueView`, la carte `Client affecté au panier` est désormais masquée pour tout utilisateur dont `CurrentAccount.Type == "Client"`.
   - La logique de validation du compte affecté au panier reste conservée en arrière-plan, mais la zone de sélection/affichage du client n'est plus visible pour un client connecté.
   - `NewReturnView` conserve le comportement existant correct : la carte client est liée à `CanEditAccount`, qui n'est vraie que pour un administrateur.
   - `OrderBasketView` conserve le masquage déjà appliqué de la ligne client dans `Mon panier` pour les utilisateurs de type Client.
   - `ReturnBasketView` ne contient pas de zone de sélection/affichage de compte client à masquer ; le champ `Code retour client` reste visible car il s'agit d'une référence métier de retour, pas d'un compte client.
   - Un client gestionnaire de commandes reste un utilisateur de type Client pour cette règle d'affichage : `IsClientOrderManager` ne doit pas réafficher ces zones.

4. Documentation et suivi
   - Les fichiers de suivi projet ont été mis à jour.
   - Le document de transfert / CDC a été mis à jour avec les corrections Version62.
   - La documentation service web par scénarios a été actualisée pour préciser les règles Version62 : fin de chemin TecDoc, masquage des zones client et affichage du motif/image des retours refusés.

Contraintes respectées :
- Aucun changement de `Reference.cs`.
- Aucun service web, champ, fallback ou comportement métier non fourni n'a été inventé.
- Les valeurs métier retournées par les services web ne sont pas recalculées ni réinterprétées.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## État de transfert après Version62

- Dernière version générée : `AGRA-EASY-MOBILE-Version62.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version61.zip`.
- Les demandes en attente jusqu'à Version61 ont été reprises : correction de la hauteur des boutons motif/image, message de fin de chemin TecDoc, et masquage effectif de la carte client Catalogue pour les utilisateurs Client.
- Les comportements déjà corrects dans `NewReturnView`, `OrderBasketView` et `ReturnBasketView` ont été contrôlés et documentés.
- La documentation service web par scénarios doit continuer à être maintenue à chaque nouvelle version impactant les scénarios ou services utilisés.
- Ne jamais générer de ZIP sans demande explicite.

## Version63 - base Version62-Update1

Base de travail : `AGRA-EASY-MOBILE-Version62-Update1.zip`, fournie avec référence service web mise à jour.

Corrections appliquées dans Version63 :

1. Vue remboursement fournisseur
   - Remplacement du placeholder `SupplierRefundView` par une vraie vue de suivi des remboursements fournisseurs.
   - La vue reprend l'esprit visuel et fonctionnel de `RefusedReturnListView`, mais avec les services et propriétés spécifiques aux remboursements fournisseurs.
   - L'entrée existante `Remb. Fourn.` / `Remboursement fournisseur` est conservée.

2. Services web remboursement fournisseur
   - Ajout des wrappers ciblés dans `EasySession.cs` :
     - `GetSupplierRefundsLinesAsync(ReturnFilter f)`.
     - `GetSupplierRefundLineAsync(string supplierRefundLineCode, string warehouse)`.
     - `GetSupplierRefundDocumentAsync(string supplierRefundLineCode, string warehouse)`.
   - Aucun changement de `Reference.cs`, `ExecuteWithRetryAsync`, `OpenConnectionAsync` ou de la logique de connexion/reconnexion.

3. Paramètre `WithTreated`
   - Ajout de la propriété `WithTreated` au modèle `ReturnFilter`.
   - La vue `SupplierRefundView` affiche une case à cocher `Traité`.
   - La valeur de cette case est transmise au service `GetSupplierRefundsLines` via le paramètre `WithTreated`.
   - Cette case sert uniquement à inclure ou non les lignes déjà traitées dans le suivi des remboursements fournisseurs.

4. Affichage des cartes remboursement fournisseur
   - Chaque carte affiche le code de remboursement fournisseur, la plateforme et la date de création.
   - Pour les utilisateurs de type `Administrateur`, `StatusCode` est affiché à côté de la plateforme.
   - La ligne principale affiche `ReturnCode`, `SupplierResponse` centré, puis `ProductCode`.
   - `SupplierResponse` est cliquable et ouvre le PDF du bon de remboursement fournisseur via le module existant `PdfViewerPage`.
   - La ligne suivante affiche `ReturnClientCode` et la quantité.
   - `SupplierComment` est affiché dans une zone encadrée sur deux lignes, avec un bouton pour afficher le commentaire complet.

5. Détail et documents
   - Le bouton de commentaire complet appelle obligatoirement `GetSupplierRefundLine(SupplierRefundLineCode, Warehouse)` avant affichage, même si les données sont déjà présentes dans la liste.
   - Le bouton fichier fournisseur est visible uniquement si `FilePath` est renseigné.
   - `FilePath` correspond au justificatif fourni par le fournisseur ; il n'est pas confondu avec le PDF généré par AGRA.
   - Le PDF généré par AGRA est récupéré via `GetSupplierRefundDocument(SupplierRefundLineCode, Warehouse)` et affiché avec le module PDF existant.

6. Documentation projet
   - Mise à jour du fichier de suivi projet.
   - Mise à jour du document de transfert / CDC / règles.
   - Mise à jour de la documentation service web par scénarios, intégrée dans le projet, avec ajout du scénario homogène `Suivi des remboursements fournisseurs`.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## État de transfert après Version63

- Dernière version générée : `AGRA-EASY-MOBILE-Version63.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version62-Update1.zip`.
- La vue `SupplierRefundView` est désormais une vraie vue de suivi des remboursements fournisseurs, utilisant les trois services web dédiés.
- La documentation service web par scénarios est homogène, sans logique chronologique, et inclut le scénario `Suivi des remboursements fournisseurs`.
- La prochaine reprise de projet doit repartir de Version63 ou d’une version ultérieure explicitement fournie par l’utilisateur.
- Ne jamais générer de ZIP sans demande explicite.


## Version64 - base Version63

Corrections appliquées dans Version64 :

1. Vue remboursements fournisseurs
   - `SupplierResponse` n'est plus ouvrable/cliquable lorsque sa valeur est `En attente`.
   - Lorsque `SupplierResponse` contient une autre valeur, l'ouverture du bon PDF de remboursement fournisseur via `GetSupplierRefundDocument` reste disponible.
   - Ajout d'un mode de tri `État`, basé sur la propriété `SupplierResponse`.

2. Modes de tri des vues de suivi
   - Les sélecteurs de mode de tri des vues `DeliveryListView`, `OrderListView`, `RuptureListView`, `ReturnListView`, `ContainerListView`, `RefusedReturnListView` et `SupplierRefundView` utilisent désormais le contrôle `BorderlessPicker`.
   - Objectif : supprimer le trait noir natif sous le sélecteur de tri et centrer le texte du mode de tri horizontalement et verticalement.
   - La personnalisation native du `BorderlessPicker` est renforcée pour Android et prévue aussi pour iOS / MacCatalyst / Windows via les handlers MAUI.

3. Documentation projet
   - Le présent suivi est mis à jour.
   - Le document de transfert / CDC est mis à jour.
   - La documentation service web par scénarios est intégrée au projet et ajustée pour préciser que le PDF de remboursement fournisseur n'est demandé que lorsque la réponse fournisseur n'est pas `En attente`.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible. Les contrôles réalisés sont statiques : cohérence des fichiers, XAML/XML, recherche textuelle, rendu visuel des documents Word et intégrité du ZIP.

## État de transfert après Version64

- Dernière version générée : `AGRA-EASY-MOBILE-Version64.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version63.zip`.
- Les corrections actives concernent le comportement `SupplierResponse`, le tri par état des remboursements fournisseurs et l'harmonisation visuelle des sélecteurs de tri.
- La prochaine reprise de projet doit repartir de Version64 ou d'une version ultérieure explicitement fournie par l'utilisateur.
- Ne jamais générer de ZIP sans demande explicite.

## Version65 - base Version64

Corrections appliquées dans Version65 :

1. Modes de tri des vues de suivi
   - Reprise de l'affichage des sélecteurs de mode de tri sur l'ensemble des vues concernées : `DeliveryListView`, `OrderListView`, `RuptureListView`, `ReturnListView`, `ContainerListView`, `RefusedReturnListView` et `SupplierRefundView`.
   - Le texte affiché dans le mode de tri est désormais rendu par un libellé centré horizontalement et verticalement à l'intérieur de l'encadrement.
   - Le contrôle `Picker` natif reste présent uniquement comme zone d'interaction transparente, afin de conserver l'ouverture de la liste de choix tout en supprimant visuellement le trait noir natif et les problèmes d'alignement.
   - La correction est appliquée de manière homogène aux vues de suivi existantes, sans modifier les règles de tri ni les données affichées.

2. Documentation projet
   - Le présent suivi est mis à jour.
   - Le document de transfert / CDC est mis à jour.
   - La documentation service web par scénarios est conservée dans le projet ; aucune modification fonctionnelle service web n'a été nécessaire pour cette version.

Points non traités dans Version65 :

- L'anomalie observée sur `SupplierRefundView` concernant l'affichage de `CreationDate` à `01/01/0001 00:00` doit rester à investiguer séparément si l'utilisateur demande une correction. La comparaison statique avec `RefusedReturnListView` n'a pas montré de traitement différent côté binding XAML, mais le comportement observé en exécution reste à analyser.
- Les règles d'affichage de `SupplierComment` et `FilePath` restent celles validées par l'utilisateur : la carte se base sur les valeurs présentes dans la liste `GetSupplierRefundsLines`, et l'appel `GetSupplierRefundLine` est réservé au clic sur le bouton d'affichage du commentaire complet.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible. Les contrôles réalisés sont statiques : cohérence des fichiers, XAML/XML et intégrité du ZIP.

## État de transfert après Version65

- Dernière version générée : `AGRA-EASY-MOBILE-Version65.zip`.
- Base utilisée : `AGRA-EASY-MOBILE-Version64.zip`.
- Les corrections actives concernent uniquement l'affichage homogène et centré des modes de tri.
- La prochaine reprise de projet doit repartir de Version65 ou d'une version ultérieure explicitement fournie par l'utilisateur.
- Ne jamais générer de ZIP sans demande explicite.

## Version66 - base Version65

Base de travail : `AGRA-EASY-MOBILE-Version65.zip`.

Correction appliquée dans Version66 :

1. Référence service web
   - Remplacement complet du fichier généré `Connected Services/Services/Reference.cs` par la référentielle complète fournie depuis le serveur.
   - La classe `SupplierRefundLine` intègre désormais la propriété `SupplierStatusCode`, placée entre `SupplierResponse` et `SupplierComment`, ce qui aligne la référence mobile avec le XML réellement retourné par les services de remboursements fournisseurs.
   - Objectif : éviter le décalage de désérialisation constaté sur les champs situés après `SupplierResponse`, notamment `SupplierComment`, `FilePath` et `CreationDate`.
   - Aucun contournement applicatif n'a été ajouté : la vue `SupplierRefundView` doit continuer à s'appuyer sur les valeurs retournées par `GetSupplierRefundsLines` pour l'affichage de la liste, et n'appeler `GetSupplierRefundLine` que lors des actions utilisateur prévues.

2. Documentation projet
   - `Suivi_AGRA_EASY_MOBILE_demandes.md` mis à jour.
   - `Transfert_AGRA_EASY_MOBILE_CDC_regles.docx` mis à jour.
   - `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` mise à jour pour intégrer `SupplierStatusCode` dans la description de `SupplierRefundLine`.

Contraintes respectées :
- Pas de modification de logique métier, de vue, de wrapper ou de comportement applicatif hors remplacement de la référence générée.
- Pas d'appel supplémentaire automatique à `GetSupplierRefundLine` pour enrichir les listes.
- Compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version66-Update1 - base Version66

Base de travail : `AGRA-EASY-MOBILE-Version66.zip`.

Correction appliquée dans Version66-Update1 :

1. Référence service web
   - Remplacement complet de `Connected Services/Services/Reference.cs` par la référence fournie après correction du paramètre de `GetCustomerBillingLines`.
   - La signature générée expose désormais `GetCustomerBillingLines(string CustomerBillingId, string Warehouse)` et `GetCustomerBillingLinesAsync(string CustomerBillingId, string Warehouse)`.
   - Aucun changement de logique applicative n'a été ajouté dans cette mise à jour : l'objectif est uniquement d'aligner la référence service web du projet avec la référence serveur fournie.

2. Suivi des factures client à venir
   - Le CDC du futur module `CustomerBillingListView` doit utiliser `CustomerBillingId` comme clé métier pour la récupération des lignes et du PDF de facture client.
   - Les futures implémentations devront conserver les règles déjà validées : ne pas recalculer les totaux, afficher les montants retournés par le service web, utiliser `InvoicedAccountCode` comme code client principal pour les règles de recherche.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Version67 - base Version66-Update1

Base de travail : `AGRA-EASY-MOBILE-Version66-Update1.zip`.

Corrections et évolutions appliquées dans Version67 :

1. Références service web
   - Conservation de la référence service web fournie en Version66-Update1.
   - La signature corrigée `GetCustomerBillingLines(string CustomerBillingId, string Warehouse)` est utilisée pour le détail des factures client.

2. Vue Retours refusés
   - Le tri `Par défaut` trie désormais par `CreationDate` décroissante, comme le suivi des commandes.

3. Vue Remboursement fournisseur
   - Le tri `Par défaut` trie désormais par `CreationDate` décroissante.
   - `SupplierResponse` est cliquable uniquement si `SupplierStatusCode` est renseigné et différent de `ING`.
   - La règle ne dépend plus du libellé affiché `En attente`.
   - La case `Traité` est rendue plus compacte : réduction de la largeur du bloc, cadrage de la case à gauche, réduction de l'écart entre la case et le libellé.
   - Les zones de tri restent à largeur fixe par vue, avec une largeur réduite au minimum nécessaire au libellé le plus long de la vue.

4. Nouveau filtre factures client
   - Ajout du modèle `CustomerBillingFilter` avec `FirstDate`, `LastDate`, `ProductCode`, `AccountCode`, `InvoicedAccountCode`, `ClientSorderCode`, `DeliveryNumber`, `ShowDetails`.
   - `InvoicedAccountCode` est le champ client principal pour les règles métier, équivalent au `AccountCode` des filtres expédition.
   - Pour un administrateur, la validation impose au moins `InvoicedAccountCode` ou `ProductCode`.
   - Pour un utilisateur de type Client, `InvoicedAccountCode` est prérempli avec le code client connecté et n'est pas éditable librement.

5. Nouvelle vue `CustomerBillingFilterView`
   - Duplication/adaptation de l'esprit de `ExpeditionFilterView`.
   - Critères : `De`, `Jusqu'à`, `Article`, `Code client facturé`, `Code client livré`, `N° commande client`, `N° BL / retour`, `Détail`.
   - Le libellé `N° BL / retour` est validé pour `DeliveryNumber`, car le champ peut représenter un bon de livraison ou un code retour selon le type de document.

6. Nouvelle vue `CustomerBillingListView`
   - Remplacement du placeholder par une vraie vue de suivi des factures client.
   - Fonction de liste : `GetCustomerBillingsLines(FirstDate, LastDate, ProductCode, AccountCode, InvoicedAccountCode, ClientSorderCode, DeliveryNumber, WithDetail)`.
   - Fonction de détail : `GetCustomerBillingLines(CustomerBillingId, Warehouse)`.
   - Fonction PDF : `GetCustomerBillingDocument(CustomerBillingId, Warehouse)`.
   - Le module PDF commun est réutilisé.
   - `CustomerBillingId` devient la clé principale : entête, bouton `Lignes`, bouton `PDF`, tri par facture.
   - Le tri par conteneur du suivi BL n'est pas repris.

7. Schéma de carte factures client
   - En-tête strictement sur une seule ligne : `CustomerBillingId` justifié à gauche, `Warehouse` centré, `CreationDate` justifiée à droite.
   - Aucun libellé `Fact.` ou `Avoir` n'est ajouté ; le type de document `Invoice` / `Refund` n'est pas affiché.
   - En mode entête, affichage des totaux facture `TotalHTBaseLine`, `TotalTVALine`, `TotalTTCLine` tels que retournés par le service.
   - En mode détail, affichage des totaux ligne `TotalHTBaseLine`, `TotalTVALine`, `TotalTTCLine` tels que retournés par le service.
   - Aucun total n'est recalculé côté application.

8. Zone client des factures client
   - Le client livré est affiché sous la forme `AccountCode - AccountName` avec `ClientNetworkCode` prioritaire à droite.
   - Si le client facturé est différent du client livré, une ligne supplémentaire est affichée avec `InvoicedAccountCode - InvoicedAccountName` selon la même logique.
   - Dans ce cas, la zone client est affichée même pour un utilisateur connecté de type Client.
   - Pour un administrateur, la zone client est affichée systématiquement.

9. Bouton `Lignes` des factures client
   - Le clic charge les lignes via `GetCustomerBillingLines(CustomerBillingId, Warehouse)`.
   - L'entête sélectionnée est conservée une seule fois en haut de la vue, comme dans le suivi des BL.
   - L'entête n'est pas répétée pour chaque ligne de détail.
   - Les totaux de facture de l'entête sélectionnée restent ceux retournés par la liste ; ils ne sont jamais recalculés à partir des lignes.

10. Détail des factures client
   - Ligne 1 : `ProductCode`, `DeliveryType`, `Quantity`, avec priorité à `ProductCode` et `Quantity`.
   - Ligne 2 : `ProductLabel`.
   - Ligne 3 : `DeliveryNumber`, `InvoiceDate`, `SorderClientCode`, avec priorité à `DeliveryNumber` et `InvoiceDate`.
   - `SorderClientCode` tronqué peut être affiché complet par tap ; un second tap masque le message.
   - Ligne 4 : `TotalHTBaseLine`, `TotalTVALine`, `TotalTTCLine`.
   - En mode détail direct, le bouton PDF n'est pas répété par ligne, sauf demande contraire future.

11. Classe `CustomerBillingLine` - description à conserver pour reprise projet
   - Propriétés d'entête facture : `CustomerBillingId` numéro de facture unique dans une plateforme, `CreationDate` date de facture/date comptable, `AccountCode` code client livré, `AccountName` nom client livré, `InvoicedAccountCode` code client facturé, `InvoicedAccountName` nom client facturé, `Type` valeur `Invoice` ou `Refund`, `Warehouse` plateforme génératrice, `ClientNetworkCode` code réseau client.
   - Propriétés de détail facture : `CustomerBillingDetailId` numéro de ligne unique dans une plateforme, `ProductCode`, `ProductLabel`, `ProductFamilyCode`, `ProductFamilyLabel`, `SorderClientCode`, `DeliveryNumber` numéro de BL ou code retour selon le type de document, `InvoiceDate` date de livraison marchandise, `DeliveryType`, `Quantity`, `Price`, `Discount`, `HTLine`, `GuaranteeFeeRate`, `OperatingCostRate`, `HtChargeLine`, `HTBrutLine`, `InvoiceDiscount`, `HTInvoiceDiscountLine`, `HTBaseLine`, `TVA`, `TVALine`, `TTCLine`.
   - Propriétés de totaux selon contexte : `TotalHTLine`, `TotalHTChargeLine`, `TotalHTBrutLine`, `TotalHTInvoiceDiscountLine`, `TotalHTBaseLine`, `TotalTVALine`, `TotalTTCLine` représentent soit les totaux facture, soit les totaux de ligne selon que l'objet représente une entête ou une ligne de détail. Ces valeurs doivent toujours être affichées telles que retournées par le service, sans recalcul.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.


## Génération suivante - corrections et évolutions intégrées

Demandes intégrées dans la version suivante :

- Filtres retours refusés et remboursements fournisseurs : période par défaut sur une semaine glissante.
- Validation des filtres administrateur : accepter tout critère métier utile autre que les dates, en excluant `SorderType`, `ReturnStatus` et `ProductStatus` comme critères débloquants. Application aux filtres expédition, retours, retours refusés, remboursements fournisseurs et factures client.
- Facturation : appel unique à `IsCustomerBillingManager` après connexion, mémorisation du droit en session, masquage du menu facturation et blocage des accès directs aux vues facturation si le droit est absent.
- Suivi des BL : ajout d’un raccourci vers le panier retour dans l’entête et d’un bouton sur les lignes détaillées de BL pour ajouter la quantité retournable au panier retour. La quantité retournable reprend l’algorithme de Nouveau retour : quantité livrée - quantité déjà retournée. Le rapprochement utilise plateforme, BL, article et conteneur lorsque celui-ci est exploitable ; conteneur vide ou 18 zéros ignoré.
- Suivi des BL : message de confirmation après ajout au panier retour avec rappel que le panier doit être validé et bouton permettant d’aller directement au panier retour.
- Factures client : suppression définitive de la case Détail de la vue d’édition du filtre ; la valeur `ShowDetails` reste pilotée par la case Détail de l’entête de la vue de suivi.
- Factures client : design de la case Détail rendu compact comme la case Traité des remboursements fournisseurs.
- Factures client : si client facturé différent du client livré, afficher une ligne supplémentaire ; pour cette ligne, ne pas afficher le code réseau, mais la mention `Facturé`. Dans ce cas, la zone client reste visible même pour un utilisateur de type Client.
- Factures client : dans la troisième ligne de détail, `DeliveryNumber` reste à gauche, `SorderClientCode` est centré et occupe l’espace disponible, `InvoiceDate` est justifiée à droite. `SorderClientCode` est tronqué si nécessaire et affichable/masquable au tap.
- Factures client : `ProductLabel` est tronqué proprement et affichable/masquable au tap selon le même principe que `SorderClientCode`.
- Documentation projet et documentation service web mises à jour avant génération de l’archive.


## Précision CDC — Ajout d’une ligne de BL au panier retour

Règle corrigée pour l’ajout depuis `DeliveryListView` :

- Avant de rechercher la ligne retournable, récupérer et traiter le panier retour existant.
- Pour tout utilisateur, y compris un utilisateur de type `Client`, vérifier que le panier retour ne contient pas déjà des lignes d’une plateforme différente de celle de la ligne de BL à retourner.
- Si l’utilisateur connecté est de type `Administrateur`, gérer aussi l’affectation du client au panier retour :
  - si le panier retour est vide, affecter au panier le client livré de la ligne de BL à retourner ;
  - si le panier retour n’est pas vide, récupérer le client déjà affecté au panier et vérifier qu’il correspond au client livré de la ligne de BL à retourner ;
  - si le client ne correspond pas, bloquer l’ajout et afficher un message propre indiquant que le panier contient déjà des lignes pour un autre client.
- Après ces contrôles seulement, appeler la récupération des lignes retournables de l’article avec le bon contexte de panier retour.
- Reprendre l’algorithme de `NewReturnView` pour la quantité retournable : quantité retournable = quantité livrée - quantité déjà retournée.
- Le filtrage de la ligne retournable se fait ensuite par plateforme, numéro de BL, article et conteneur si le conteneur est exploitable.
- Si le conteneur est vide ou vaut 18 zéros d’un côté ou de l’autre, il ne doit pas bloquer le rapprochement.
- Le raccourci vers le panier retour dans l’en-tête de la vue BL doit utiliser l’icône choisie n°8 (`📦➕`) et ne doit pas être placé entre le mode de tri et le sens de tri.
- L’action d’ajout d’une ligne au retour doit être représentée par une petite icône choisie n°10 (`⤴🛒`), placée sur la ligne conteneur/quantité, entre la fin du conteneur et la quantité.
- Le gros bouton texte `Ajouter au retour` doit être remplacé par cette petite action pour éviter d’alourdir les cartes.


## Version69 - corrections appliquées

- Reconstruction logique du Shell corrigée : le passage de aucun utilisateur connecté à un utilisateur connecté est désormais considéré comme un changement d’état utilisateur, ce qui force la reconstruction du Shell après connexion réussie et permet d’appliquer les droits comme la facturation.
- Suivi BL : le raccourci vers le panier retour utilise l’icône choisie n°8 (`📦➕`) et n’est plus placé entre le mode de tri et le sens du tri.
- Suivi BL : le bouton texte `Ajouter au retour` est remplacé par une petite action avec l’icône choisie n°10 (`⤴🛒`) sur la ligne conteneur/quantité.
- Suivi BL : l’algorithme d’ajout au panier retour récupère désormais le panier en premier, vérifie la plateforme pour tous les utilisateurs, gère le client du panier pour les administrateurs avant la recherche retournable, puis recherche et filtre la ligne retournable.
- Le calcul de quantité retournable reste aligné avec `NewReturnView` : quantité livrée moins quantité déjà retournée.
- Documentation et fichiers de suivi intégrés dans l’archive.

Limite : compilation réelle non exécutée dans l’environnement car `dotnet` n’est pas disponible.


## Précision validée — Panier retour et raccourci depuis le suivi BL

- La correction de `ReturnBasketView` est validée : pour un utilisateur de type Administrateur, afficher une section `Client affecté au panier retour`.
- Cette section doit afficher le client actuellement affecté au panier retour.
- Si le panier retour est vide, l’administrateur peut choisir ou modifier le client.
- Si le panier retour contient déjà des lignes, le client affecté doit être affiché mais ne doit pas être modifié, afin d’éviter de mélanger des lignes de clients différents.
- Le raccourci vers le panier retour dans l’en-tête de `DeliveryListView` ne doit plus utiliser une icône composée.
- Le raccourci doit afficher uniquement l’icône colis/carton (`📦`), sans signe plus.
- L’icône d’ajout au retour au niveau de la ligne BL est validée telle qu’elle est actuellement ; ne pas la modifier.


## Correction de précision — icône du raccourci panier retour

Correction de la précision précédente : le raccourci vers le panier retour dans l’en-tête de `DeliveryListView` ne doit pas utiliser l’icône colis/carton.

Règle validée :
- reprendre le **caddie** de l’icône déjà utilisée sur le bouton de ligne `Ajout au panier retour` ;
- utiliser ce caddie comme icône du bouton d’en-tête servant de raccourci vers le panier retour ;
- ne pas modifier l’icône du bouton de ligne, qui reste validée telle qu’elle est.


## Version70 — corrections panier retour et raccourci BL

Corrections intégrées :
- `ReturnBasketView` affiche maintenant une section `Client affecté au panier retour` pour les utilisateurs de type Administrateur.
- Si le panier retour est vide, l’administrateur peut choisir ou modifier le client affecté au panier.
- Si le panier retour contient déjà des lignes, le client affecté est affiché mais ne peut pas être modifié, afin d’éviter de mélanger des lignes de clients différents.
- Le raccourci vers le panier retour dans l’en-tête de `DeliveryListView` reprend le caddie de l’icône du bouton de ligne.
- L’icône d’ajout au retour au niveau de la ligne BL n’a pas été modifiée.

Contrôles :
- XAML contrôlé statiquement.
- Compilation réelle non exécutée dans l’environnement de génération, car `dotnet` n’est pas disponible.


## Précision CDC — démarrage hors Shell et auto-authentification

- Au lancement de l’application, ne pas créer `AppShell` directement.
- Afficher une page indépendante du Shell dédiée au démarrage/connexion, par exemple `StartupConnectionView` ou `ConnectionBootstrapPage`.
- Cette page doit tenter automatiquement l’authentification avec les paramètres enregistrés.
- Si l’authentification automatique réussit, la page de démarrage ne doit pas rester affichée : l’application doit créer et afficher directement `AppShell`.
- Le Shell doit donc être construit uniquement après une connexion réussie et après récupération des droits utilisateur nécessaires, notamment `IsCustomerBillingManager`.
- Si l’authentification automatique échoue ou si les paramètres sont absents/incomplets, la page indépendante doit afficher la vue de paramètres/connexion.
- Après une connexion manuelle réussie depuis cette page, créer immédiatement le Shell configuré avec les droits connus.
- L’objectif est d’éviter qu’un Shell soit construit avant connexion, avec des menus ou vues déjà initialisés avec des droits par défaut incorrects.


## Précision CDC — maintien de la reconnexion automatique après échec d’appel service web

La future refonte du démarrage hors `AppShell` ne doit pas modifier la logique de récupération de session existante.

Règle à conserver :
- si un appel de service web échoue pour une raison compatible avec une perte de session ou une connexion rompue, l’application doit retenter une authentification avec les paramètres enregistrés ;
- si la réauthentification réussit, l’appel initial doit pouvoir être retenté selon la logique existante ;
- si la réauthentification échoue, l’application doit revenir vers la page indépendante de connexion/paramètres ;
- le retour vers la page de connexion doit détruire/remplacer le Shell courant si nécessaire, afin d’éviter de conserver des vues ou menus construits avec un ancien état utilisateur ;
- cette logique ne doit pas être confondue avec le nouveau démarrage hors Shell : le démarrage hors Shell concerne seulement l’initialisation de l’application, tandis que la reconnexion automatique concerne les appels service web en cours d’utilisation.


## Version71 — démarrage hors Shell et reconnexion conservée

Corrections intégrées :
- `AppShell` n’est plus créé directement au démarrage de l’application.
- `App.CreateWindow` ouvre maintenant une page indépendante `StartupConnectionView`.
- `StartupConnectionView` tente automatiquement l’authentification avec les paramètres enregistrés.
- Si l’authentification automatique réussit, la page de démarrage est remplacée immédiatement par `AppShell`.
- `AppShell` est donc construit après connexion réussie et après récupération des droits utilisateur, notamment `IsCustomerBillingManager`.
- Si les paramètres sont absents ou si l’authentification échoue, la page de connexion/paramètres est affichée.
- `ValidateAndNavigateAsync` fonctionne maintenant même hors Shell : après connexion réussie, il crée ou remplace le Shell si nécessaire.
- La logique de reconnexion après échec d’appel service web est conservée : tentative de réauthentification, puis retour vers la page de connexion/paramètres si la réauthentification échoue.
- En cas de retour à la page de connexion après échec de réauthentification, le Shell courant est remplacé afin d’éviter de garder des vues ou menus construits avec un ancien état utilisateur.

Contrôles :
- XAML contrôlé statiquement.
- Compilation réelle non exécutée dans l’environnement de génération, car `dotnet` n’est pas disponible.


## Version72 — version de transmission depuis Version71(2)

Base de travail : `AGRA-EASY-MOBILE-Version71(2).zip`, fournie par l'utilisateur comme dernière base réelle à utiliser.

Objectif : générer une version de transmission complète pour basculer vers une nouvelle discussion sans perdre les règles, le contexte, les demandes en attente et les demandes mises de côté.

Actions réalisées :

- Ajout du fichier `REPRISE_NOUVELLE_DISCUSSION_AGRA_EASY_MOBILE.md`, à lire en premier dans la prochaine discussion.
- Vérification de la présence des évolutions majeures depuis Version67 dans la base fournie : factures client, droits facturation, correction de référence `GetCustomerBillingLines(CustomerBillingId, Warehouse)`, panier retour depuis suivi BL, démarrage hors Shell et reconnexion conservée.
- Confirmation que les demandes fonctionnelles générées entre Version67 et Version71 sont présentes dans le suivi ou intégrées à la base fournie.
- Confirmation que la demande de création future d'une facade REST/JSON depuis le SOAP existant est classée dans les demandes mises de côté, et non comme demande active de prochaine version.
- Recopie des règles générales de développement et règles de transmission dans le fichier de reprise.
- Conservation de la documentation service web par scénarios dans le ZIP.
- Aucun changement fonctionnel de code n'a été volontairement apporté dans cette version de transmission.

Etat pour reprise :

- Prochaine base à utiliser : `AGRA-EASY-MOBILE-Version72.zip` ou version ultérieure explicitement fournie.
- Demandes fonctionnelles actives à générer automatiquement : aucune à ce stade.
- Demande mise de côté : facade REST/JSON limitée aux services réellement utilisés par l'application mobile, à reprendre seulement sur demande explicite.

Limites : compilation réelle non exécutée dans l'environnement car `dotnet` n'est pas disponible.

## Demandes mises de côté jusqu'à nouvel ordre

### Facade REST/JSON limitée au périmètre mobile

Demande classée hors prochaine version : créer plus tard une facade REST/JSON équivalente aux services SOAP réellement utilisés par l'application mobile AGRA-EASY-MOBILE.

Règles de périmètre :

- couvrir uniquement les scénarios/services réellement utilisés par l'application mobile ;
- ne pas convertir toute la référence SOAP ;
- ne pas convertir tout l'ASMX ;
- ne pas inclure les fonctions non consommées par l'application mobile ;
- figer des contrats HTTP/JSON stables pour préparer une migration future du serveur vers ASP.NET moderne / .NET 10 ;
- reprendre ce chantier uniquement si l'utilisateur fournit le code ASMX et le demande explicitement.


## Version73 — suivi de la facturation en attente et référence services web

Base de travail : `AGRA-EASY-MOBILE-Version72(1).zip`, fournie par l'utilisateur comme nouvelle base opérationnelle.

Actions réalisées :

- Remplacement de `Connected Services/Services/Reference.cs` par le nouveau fichier de référence service web fourni.
- Ajout de la prise en charge de `GetInvoiceWaitingLines` et de la classe `InvoiceWaitingLine` fournis par la nouvelle référence.
- Ajout de `DeliveryType` dans `ExpeditionFilter` pour permettre au même filtre d'expédition de fonctionner en mode `Type de livraison` dans le module de facturation en attente.
- Mise à jour de `ExpeditionFilterView` :
  - ajout de la valeur `EXTERNE` dans la liste des types de commande du mode expédition classique ;
  - ajout d'un mode `InvoiceWaiting` où le champ `Type de commande` devient `Type de livraison` ;
  - valeurs `DeliveryType` transmises au service : `DEPOT`, `EXPRESS`, `MAGASIN`, `STOCK`, `IMPLANTATION`, `REFACTURATION`, `DROP-SHIPPING`, `PORT`, `MANUEL`, `REGULARISATION`, `PSD`, `PERIODIC` ;
  - libellés affichés plus explicites pour `PORT` (`FORFAIT DE FRAIS DE PORT`) et `PERIODIC` (`PÉRIODIQUE`) tout en conservant les valeurs techniques transmises au service ;
  - normalisation de `ContainerNo` : si la saisie non vide fait 18 caractères ou moins, complétion à gauche avec des zéros jusqu'à 18 caractères ; si elle dépasse 18 caractères, conservation telle quelle.
- Remplacement du placeholder `InvoiceWaitingListView` par un module réel de suivi de la facturation en attente.
- Ajout du wrapper `EasySession.GetInvoiceWaitingLinesAsync(ExpeditionFilter f)` avec passage des paramètres `FirstDate`, `LastDate`, `AccountCode`, `ProductCode`, `ClientSorderCode`, `DeliveryType`, `ContainerNo`, `DeliveryNumber`.
- Ajout de `Models/InvoiceWaitingLine.partial.cs` pour l'affichage formaté des valeurs retournées.
- Affichage du module :
  - entête : `InvoiceWaitingCode`, `Warehouse / DeliveryType` sur la même ligne, puis `InvoiceDate` ou à défaut `CreationDate` ;
  - montant : `Price` avec `Discount` en petit pourcentage en haut à droite du prix ;
  - addition réelle `ConsigneValue + EcotaxeValue`, avec valeurs nulles traitées comme zéro ;
  - affichage de `TTC` ;
  - affichage du `AccountCode` uniquement pour un utilisateur Administrateur ;
  - détail : `ProductCode`, `DeliveryNumber`, `Quantity`, puis `ProductLabel`, puis `SorderClientCode`, `TVA`, `LineTotal` ;
  - `SorderClientCode` peut être tronqué et affiché/masqué par appui.
- Modes de tri du module : `Date création`, `Date livraison` avec repli sur `CreationDate`, `N° attente`, `Client`, `Article`, `N° commande`, `N° BL`.
- Mise à jour de `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` avec le scénario de facturation en attente et les valeurs `DeliveryType`.
- Mise à jour de `Transfert_AGRA_EASY_MOBILE_CDC_regles.docx` avec les règles du nouveau module.

Contrôles :

- Contrôle XML effectué sur `Views/InvoiceWaitingListView.xaml` et `Views/ExpeditionFilterView.xaml`.
- Contrôle statique des signatures `InvoiceWaitingLine` et `GetInvoiceWaitingLines` dans la référence remplacée.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

## Version74 — correction filtre expédition avec types indépendants

Base de travail : `AGRA-EASY-MOBILE-Version73.zip`, générée dans cette discussion.

Contexte : après la version 73, les modules utilisant `ExpeditionFilterView` pouvaient planter sous Android avec une exception enveloppée dans `Android.Runtime.JavaProxyThrowable`. Le point suspect confirmé concerne la gestion du champ `Type de commande` / `Type de livraison`.

Correction intégrée :

- Remplacement de la gestion dynamique du contenu du même picker par deux contrôles indépendants définis statiquement dans le XAML :
  - `PickerSorderType` pour les modules d’expédition classiques ;
  - `PickerDeliveryType` pour le module de facturation en attente.
- Conservation de `PickerSorderType` avec les valeurs statiques de type de commande : vide, `STOCK`, `MAGASIN`, `EXPRESS`, `EXTERNE`.
- Ajout de `PickerDeliveryType` avec les valeurs statiques de type de livraison affichées dans le XAML : vide, `DEPOT`, `EXPRESS`, `MAGASIN`, `STOCK`, `IMPLANTATION`, `REFACTURATION`, `DROP-SHIPPING`, `FORFAIT DE FRAIS DE PORT`, `MANUEL`, `REGULARISATION`, `PSD`, `PÉRIODIQUE`.
- Affichage exclusif du contrôle correspondant au mode appelant la vue :
  - mode expédition classique : `PickerSorderType` visible et actif, `PickerDeliveryType` masqué ;
  - mode facturation en attente : `PickerDeliveryType` visible et actif, `PickerSorderType` masqué.
- Suppression de la logique qui vidait/remplissait dynamiquement `PickerSorderType.Items`, afin d’éviter le mélange entre `ItemsSource` XAML et manipulation de `Items` côté code.
- Conservation de la conversion technique pour le service `GetInvoiceWaitingLines` :
  - libellé affiché `FORFAIT DE FRAIS DE PORT` transmis sous la valeur `PORT` ;
  - libellé affiché `PÉRIODIQUE` transmis sous la valeur `PERIODIC`.
- Préservation de la logique existante de `SorderType` pour les autres modules utilisant le filtre d’expédition.

Règle de livraison ajoutée :

- Ne pas fournir de fichiers sources partiels, patchs, extraits de code ou correctifs isolés comme livrables téléchargeables.
- Générer un ZIP complet du projet uniquement sur demande explicite de l’utilisateur, avec incrément du numéro de version.
- Pour les demandes planifiées ou en attente, fournir systématiquement un fichier de suivi téléchargeable séparé et intégrer ce même fichier dans le dossier projet.

Contrôles :

- Contrôle XML effectué sur `Views/ExpeditionFilterView.xaml`.
- Contrôle statique effectué pour vérifier l’absence de manipulation dynamique `PickerSorderType.Items.Clear()` / `Items.Add()` dans `ExpeditionFilterView.xaml.cs`.
- Contrôle statique effectué pour vérifier que les deux pickers sont indépendants et que seul le picker utile est visible selon le mode.
- Compilation réelle non exécutée dans l’environnement de génération, car `dotnet` n’est pas disponible.


## Version75 — facturation en attente et remboursements en attente

Base de travail : `AGRA-EASY-MOBILE-Version74.zip`, générée dans cette discussion.

Actions réalisées :

- Module `Facturation en attente` :
  - ajout du préfixe `N°` devant le numéro de facturation en attente ;
  - tri par défaut défini sur la date de création décroissante ;
  - remplacement de la légende `Prix` par `Prix BL :` ;
  - réagencement de la deuxième ligne d'entête avec une répartition dynamique entre `Prix BL`, `Cons./éco` et `TTC` ;
  - réagencement de la première ligne de détail pour laisser le code article prendre davantage d'espace et pousser le numéro de BL si nécessaire, tout en conservant la quantité prioritaire ;
  - réagencement de la troisième ligne de détail sur le même principe pour `Cmd`, `TVA` et `Total` ;
  - ajout du préfixe `Cmd :` devant le numéro de commande client.
- Panier de retour :
  - affichage de la plateforme au milieu de la ligne qui contient le numéro de BL.
- Nouveau module `Remboursements en attente` :
  - remplacement du placeholder `RefundInvoiceListView` par un module réel ;
  - module accessible via l'onglet `À rembourser` de la zone facturation ;
  - utilisation du filtre retour avec les deux filtres d'état masqués, comme pour les retours refusés ;
  - ajout du wrapper `EasySession.GetRefundWaitingLinesAsync(ReturnFilter f)` ;
  - appel du service `GetRefundWaitingLines` avec les paramètres `FirstDate`, `LastDate`, `AccountCode`, `ProductCode`, `ReturnCode`, `ReturnClientCode` ;
  - vérification de la référence générée : la classe retournée s'appelle `RefundWatingLine` dans `Reference.cs`, avec l'orthographe réellement générée ;
  - ajout de `Models/RefundWatingLine.partial.cs` pour l'affichage formaté des valeurs retournées ;
  - affichage inspiré de la facturation en attente avec `RefundWaitingCode`, `Warehouse / ReturnType`, date, `DeliveryNetPrice`, `Depression`, `AdditionalDepression`, `TTC`, `AccountCode`, `ProductCode`, `DeliveryNumber`, `Quantity`, `ProductLabel`, `ReturnClientCode`, `TVA`, `LineTotal` ;
  - absence de légende `AdditionalDepression` lorsque la valeur retournée est nulle ;
  - tri par défaut sur date de création décroissante, avec modes de tri complémentaires similaires au module de facturation en attente.
- Documentation et transfert :
  - mise à jour du suivi de projet ;
  - mise à jour du document de transfert avec les règles du module `Remboursements en attente` ;
  - mise à jour de la documentation service web avec le scénario `GetRefundWaitingLines`.

Contrôles :

- Contrôle XML effectué sur les XAML modifiés et ajoutés.
- Contrôle statique effectué sur la signature `GetRefundWaitingLines` et sur les propriétés de `RefundWatingLine` dans `Reference.cs`.
- Contrôle statique effectué pour vérifier que le module `À rembourser` ne contient plus le placeholder de démarrage.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.


## Version76 — corrections attente et module frais de port

Base de travail : `AGRA-EASY-MOBILE-Version75.zip`, générée dans cette discussion.

Actions réalisées :

- Module `Facturation en attente` : masquage de `Cons./éco` si la somme `ConsigneValue + EcotaxeValue` est vide ou égale à zéro ; ajout de `BL :` devant le numéro de BL ; alignement de la colonne BL avec la TVA.
- Module `Remboursements en attente` : affichage de `Depression` et `AdditionalDepression` en pourcentage entier sans décimales ; masquage de `Dépr.` si les valeurs sont vides ou égales à zéro ; absence de légende `Dépr.+` si `AdditionalDepression` est nul ou égal à zéro ; ajout de `BL :` et alignement BL/TVA.
- Nouveau module `Frais de port` : remplacement de l'entrée Shell `Refacturation` par `Frais de port`, suppression du placeholder `SupplierBillingListView`, ajout de `ShippingCostFilter`, `ShippingCostFilterView`, `ShippingCostListView` et du wrapper `EasySession.GetShippingCostLinesAsync`.
- Filtre frais de port : chargement des plateformes via `GetWarehousesListV2`, affichage de `WarehouseCode`, exclusion des plateformes externes (`IsExternal = true`), et validation obligatoire d'au moins un critère parmi `AccountCode`, `ShippingCostDeliveryNumber`, `TurnoverDeliveryNumber`.
- Le filtre `DeliveryType` des frais de port a été ignoré volontairement, car il est abandonné pour la version mobile.
- Appel service : `GetShippingCostLines(FirstDate, LastDate, Warehouse, AccountCode, ShippingCostDeliveryNumber, TurnoverDeliveryNumber, WithDetail, WithBilledLines, WithExemptLines, WithWaitingLines)`.
- Entête de vue frais de port : case `Détail` décochée par défaut, tri, bouton filtre, puis états `En attente` coché par défaut, `Facturé` et `Exonéré` décochés par défaut.
- Carte frais de port : `ShippingCostCode`, `Warehouse / ShippingCostDeliveryNumber`, `ShippingDate`, `CarrierName`, `TotalWeight`, `DeliveredContainerCount`, `Cost`, adresse avec affichage complet sur appui, ville/code postal, description sur deux lignes avec affichage complet, client pour administrateur, et détail `ContainerNo`, `TurnoverDeliveryNumber`, `Weight`.
- Modes de tri : `Plateforme`, `N° BL`, `Date expédition`, `Transporteur`, `Client`.

Contrôles :

- Contrôle XML effectué sur les XAML modifiés et ajoutés.
- Contrôle statique effectué sur la signature `GetShippingCostLines` et sur les propriétés de `ShippingCostLine` dans `Reference.cs`.
- Contrôle statique effectué pour confirmer l'absence de `DeliveryType` dans le filtre frais de port mobile.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

## Version77 — ajustements affichage frais de port

Base de travail : `AGRA-EASY-MOBILE-Version76.zip`, générée dans cette discussion.

Actions réalisées :

- Module `Frais de port` : repositionnement du bouton `Lignes` à droite du bloc adresse, sur la hauteur des deux lignes d'adresse, afin de supprimer la ligne vide au milieu de la carte.
- Module `Frais de port` : recentrage de la zone `TotalWeight / DeliveredContainerCount` sur la deuxième ligne d'entête de carte, avec une répartition dynamique permettant au nom du transporteur de prendre l'espace nécessaire avant troncature.
- `ShippingCostFilterView` : dates proposées par défaut selon la règle validée :
  - du 1er au 7 inclus : date début = premier jour du mois précédent, date fin = dernier jour du mois en cours ;
  - à partir du 8 : date début = premier jour du mois en cours, date fin = dernier jour du mois en cours ;
  - les dates saisies manuellement par l'utilisateur ne sont pas modifiées.
- Ligne de détail du module `Frais de port` : ajout du préfixe `Colis : ` devant `ContainerNo`.
- Ligne de détail du module `Frais de port` : conservation de la position de `BL : TurnoverDeliveryNumber` grâce à une ligne en trois zones où le colis est tronqué à gauche si nécessaire, sans décaler le BL.

Contrôles :

- Contrôle XML effectué sur les XAML modifiés.
- Contrôle statique effectué sur les règles de dates par défaut du filtre frais de port.
- Contrôle statique effectué sur le binding `DisplayContainerNo` et sur l'agencement de la ligne de détail.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.
## Version78 — annulation des filtres sans critère initial et ajustement frais de port

Base de travail : `AGRA-EASY-MOBILE-Version77.zip`, générée dans cette discussion.

Actions réalisées :

- `CustomerBillingFilterView` : le bouton `Annuler` applique désormais le même principe que `ExpeditionFilterView` / `ReturnFilterView`. Si le filtre client a été ouvert automatiquement alors qu'aucun filtre administrateur valide n'existe, l'annulation ferme la modale puis renvoie vers l'accueil pour éviter la boucle d'ouverture automatique.
- `ShippingCostFilterView` : même correction d'annulation pour le filtre frais de port. Si aucun filtre administrateur valide n'existe encore, `Annuler` renvoie vers l'accueil après fermeture de la modale.
- Module `Frais de port` : suppression du bouton `Lignes` dans les cartes. L'affichage détaillé reste piloté par la case globale `Détail`, sans bouton par carte.
- Module `Frais de port` : déplacement du code client sur la même ligne que la ville et le code postal, aligné à droite.
- Module `Frais de port` : conservation stricte de la règle existante d'affichage du code client uniquement pour les utilisateurs de type administrateur.

Contrôles :

- Contrôle XML effectué sur `Views/ShippingCostListView.xaml` et `Views/ShippingCostFilterView.xaml`.
- Contrôle statique effectué sur les handlers `OnCancelClicked` des filtres `CustomerBillingFilterView` et `ShippingCostFilterView`.
- Contrôle statique effectué pour confirmer l'absence du bouton `Lignes` dans `ShippingCostListView.xaml`.
- Contrôle statique effectué pour confirmer que `DisplayAccountCode` reste conditionné par `ShouldShowCustomerBlock`.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.


## Version79 — corrections frais de port et photo retours refusés

Base de travail : `AGRA-EASY-MOBILE-Version78.zip`, générée dans cette discussion.

Actions réalisées :

- Module `Frais de port` : suppression de la validation non demandée qui imposait de cocher au moins un état parmi `En attente`, `Facturé` et `Exonéré`. Les trois booléens `WithWaitingLines`, `WithBilledLines` et `WithExemptLines` sont désormais transmis exactement selon le choix utilisateur, même si les trois valeurs sont `false`.
- Module `Frais de port` : la zone description est réservée systématiquement et réduite à une seule ligne. Le texte reste tronqué si nécessaire, sans deuxième ligne réservée.
- Module `Retours refusés` : ajout d’un petit bouton icône après la plateforme, visible uniquement pour les utilisateurs administrateurs, pour associer une photo au retour refusé.
- Module `Retours refusés` : ajout du wrapper `EasySession.UploadRefusedReturnPictureAsync(byte[] pictureByteTable, string receptionTracingId)` vers le service `UploadRefusedReturnPicture`.
- Module `Retours refusés` : la photo prise ou sélectionnée est convertie en JPG côté Android avant l’envoi au serveur. Le rattachement utilise l’identifiant de ligne de retour refusé disponible dans la carte.
- Plateforme Android : ajout des permissions déclaratives nécessaires à la prise de photo et à la sélection d’image.

Point d’attention :

- La prise et la sélection de photo utilisent les capacités natives MAUI/Android (`MediaPicker`) sans ajout de dépendance externe. Aucune dépendance GitHub n’a été intégrée sans validation explicite.

Contrôles :

- Contrôle XML effectué sur `Views/ShippingCostListView.xaml`, `Views/RefusedReturnListView.xaml` et `Platforms/Android/AndroidManifest.xml`.
- Contrôle statique effectué pour confirmer la suppression du message `Sélectionnez au moins un état de ligne.`.
- Contrôle statique effectué pour vérifier la présence du wrapper `UploadRefusedReturnPictureAsync` et son appel depuis `RefusedReturnListView`.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.


## Version80 — correction caméra retours refusés

Base de travail : `AGRA-EASY-MOBILE-Version79.zip`, générée dans cette discussion.

Actions réalisées :

- Module `Retours refusés` : remplacement de la boîte de choix native `MediaPicker` par une interface caméra Android dédiée et plein écran.
- L'interface caméra s'ouvre directement au clic sur le bouton photo administrateur placé après la plateforme, sans boîte intermédiaire.
- L'interface caméra affiche le flux vidéo et uniquement deux actions principales : prendre la photo immédiatement ou quitter la caméra pour sélectionner une photo existante.
- La sélection d'une photo existante passe par le sélecteur de document Android depuis l'interface caméra et convertit l'image retenue en JPG avant l'envoi.
- L'envoi au serveur conserve le wrapper `EasySession.UploadRefusedReturnPictureAsync(byte[] pictureByteTable, string receptionTracingId)` et la signature métier `UploadRefusedReturnPicture`.
- La fonction et son bouton restent réservés aux utilisateurs administrateurs.
- Permissions Android : la permission caméra est demandée uniquement au clic administrateur sur la fonction photo. Aucun prompt de permission n'est déclenché au lancement de l'application ni pour les utilisateurs non administrateurs. La permission `READ_MEDIA_IMAGES` ajoutée dans la version précédente a été retirée ; la sélection utilise le sélecteur Android avec accès ponctuel au fichier choisi.
- Aucune dépendance GitHub/NuGet externe n'a été ajoutée. L'interface caméra repose sur une activité Android native dédiée.

Contrôles :

- Contrôle XML effectué sur `Platforms/Android/AndroidManifest.xml` et `Platforms/Android/Resources/values/styles.xml`.
- Contrôle statique effectué pour vérifier la suppression de `DisplayActionSheet`, `MediaPicker`, `CapturePhotoAsync`, `PickPhotoAsync` et `READ_MEDIA_IMAGES` du flux retours refusés.
- Contrôle statique effectué pour vérifier l'appel direct à `RefusedReturnPictureService.CaptureOrPickJpegBytesAsync` depuis `RefusedReturnListView`.
- Contrôle statique effectué sur la présence de l'activité Android plein écran `RefusedReturnCameraActivity`.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

## Version81 — correction compilation caméra Android

Base de travail : `AGRA-EASY-MOBILE-Version80.zip`, générée dans cette discussion.

Actions réalisées :

- Plateforme Android : correction de `Platforms/Android/RefusedReturnCameraActivity.cs` pour lever les ambiguïtés de compilation remontées sur `Camera`, `ImageButton` et `Path`.
- Plateforme Android : utilisation explicite de `Android.Hardware.Camera` pour l’aperçu plein écran et la prise de photo.
- Plateforme Android : remplacement de l’affectation interdite à `LinearLayout.Gravity` par l’appel Android approprié `SetGravity`.
- Plateforme Android : sécurisation des accès à l’instance caméra avant `TakePicture`, `SetPreviewTexture`, `StartPreview`, `StopPreview` et `Release`.
- Le principe métier reste inchangé : bouton photo visible uniquement pour les administrateurs, permission caméra demandée uniquement au déclenchement explicite de la fonction, sélection d’image existante via le sélecteur Android, conversion JPG et envoi via `UploadRefusedReturnPicture`.
- Aucune dépendance GitHub/NuGet externe n’a été ajoutée.

Contrôles :

- Contrôle statique effectué sur les erreurs de compilation remontées par l’utilisateur : ambiguïtés `Camera`, `ImageButton`, `Path`, affectation `LinearLayout.Gravity`, et appels caméra associés.
- Contrôle statique effectué pour vérifier l’absence d’utilisation non qualifiée de `Camera`, `ImageButton` et `Path` dans `RefusedReturnCameraActivity.cs`.
- Contrôle XML effectué sur les fichiers Android déclaratifs inchangés.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.



---

## Version 82 — corrections réalisées

Base de travail : `AGRA-EASY-MOBILE-Version81.zip`.

Corrections intégrées :

- `Platforms/Android/RefusedReturnCameraActivity.cs` : suppression de l'appel invalide `SetJpegQuality(90)` et application de `ImageView.ScaleType.Center` via `button.SetScaleType(...)` afin de corriger les erreurs de compilation restantes de la caméra Android.
- `RefusedReturnListView` / `RefusedReturnLine.partial.cs` : le bouton photo reste réservé aux administrateurs et n'est visible/accessible que si la plateforme du retour refusé correspond à la plateforme courante de connexion.
- `GlobalState` : les méthodes `EnsureClientExpeditionFilter`, `EnsureClientReturnFilter`, `EnsureClientCustomerBillingFilter` et `EnsureClientShippingCostFilter` ne restaurent plus automatiquement les dates effacées par l'utilisateur.
- `GlobalState.EnsureReturnFilterWithSlidingWeek` : les dates par défaut du filtre retour ne sont appliquées que lors de la création initiale du filtre.
- `ShippingCostFilterView.LoadCurrentFilter` : suppression du fallback qui réaffichait les dates par défaut quand les dates du filtre existaient mais étaient vides.
- `CustomerBillingFilter` : à la création initiale uniquement, la date de début par défaut est le premier jour du mois précédent et la date de fin par défaut est la date du jour.
- Nettoyage du dossier projet : suppression des anciens fichiers `Suivi_AGRA_EASY_MOBILE_demandes_Version*.md` et `REPRISE_NOUVELLE_DISCUSSION_AGRA_EASY_MOBILE_Version*.md` afin de ne conserver que les fichiers stables.

Documentation services web : pas de modification du document `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` pour cette version, car aucun service, paramètre, type retourné ou propriété WebService utilisé n'a changé.

État : réalisé dans la version 82.

---

---

## Version 83 — scanner code-barres article

Base de travail : `AGRA-EASY-MOBILE-Version82.zip`.

Actions réalisées :

- Intégration de la dépendance validée `BarcodeScanning.Native.Maui` en version fixe `3.0.3` par `PackageReference`.
- Initialisation du scanner dans `MauiProgram.cs` avec `UseBarcodeScanning()`.
- Ajout des permissions déclaratives nécessaires au scanner : caméra Android déjà utilisée par le module photo, vibration Android, et description caméra iOS/MacCatalyst.
- Ajout d'une vue `BarcodeScannerPage` avec caméra plein écran, cadre de lecture, animation de ligne horizontale qui balaie verticalement et fermeture automatique dès qu'un code-barres est détecté.
- Ajout du service réutilisable `ProductBarcodeScanService` pour lancer le scanner, récupérer le code-barres et résoudre la référence article via `FindProductCodeList` avec `IsGenCode = true`.
- Si `FindProductCodeList` retourne au moins un article, le premier `ProductCode` retourné est utilisé comme vraie référence article.
- Si `FindProductCodeList` ne retourne aucune ligne, le code-barres scanné est utilisé directement comme référence article.
- Ajout de l'icône `ic_barcode_scan.svg`.
- Ajout d'un petit bouton scanner au-dessus du bouton de recherche article dans `ExpeditionFilterView`, `ReturnFilterView`, `CustomerBillingFilterView` et `ProductCodeSelectionPage`.
- Le bouton de recherche article classique reste présent et inchangé fonctionnellement, mais il est visuellement réduit pour permettre l'empilement avec le bouton scanner.
- La recherche article manuelle existante reste inchangée : les résolutions par saisie et par sélection article continuent d'utiliser `FindProductCodeList` avec `IsGenCode = false`.
- Mise à jour de la documentation des services web utilisés pour documenter le scénario `FindProductCodeList` par code-barres avec `IsGenCode = true`.

Point mis à côté / non réalisé dans cette version :

- L'autonomie complète vis-à-vis des dépôts GitHub/NuGet n'est pas intégrée dans cette version. La dépendance scanner est référencée par NuGet classique et nécessitera une restauration disponible sur le poste de compilation tant que les `.nupkg` locaux et leurs dépendances transitives ne sont pas fournis/intégrés.
- Demande maintenue à part : rendre le projet durablement compilable offline en intégrant les `.nupkg` locaux de toutes les dépendances NuGet/GitHub déjà utilisées par le projet, scanner compris.

Contrôles :

- Contrôle XML effectué sur les XAML modifiés et ajoutés.
- Contrôle XML effectué sur les manifests iOS/MacCatalyst/Android modifiés.
- Contrôle statique effectué sur les usages `OnScanProductBarcodeClicked`, `ProductBarcodeScanService`, `BarcodeScannerPage` et `FindProductCodeListAsync(..., true)`.
- Contrôle statique effectué pour vérifier que le scanner n'écrase pas la recherche article classique existante.
- Rendu visuel effectué sur les DOCX modifiés.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

État : réalisé dans la version 83.


---

## Version 84 — correction scanner dans la page de sélection article

Base de travail : `AGRA-EASY-MOBILE-Version83.zip`.

Correction intégrée :

- `Views/ProductCodeSelectionPage.cs` : ajout de la méthode manquante `ScanProductBarcodeAsync()` appelée par le bouton scanner ajouté en V83.
- La méthode appelle `ProductBarcodeScanService.ScanAndResolveProductAsync(this)`.
- Si un article est résolu par le scan, la page modale se ferme en retournant directement cet article à l'appelant.
- Si le scan est annulé ou ne retourne rien, la page de sélection reste inchangée.
- La recherche article classique, la sélection manuelle et les boutons scanner déjà ajoutés dans les filtres ne sont pas modifiés.

Documentation services web : pas de modification du document `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` pour cette version, car le scénario `FindProductCodeList(..., IsGenCode = true)` est déjà documenté et aucun service, paramètre, type retourné ou propriété WebService utilisé n'a changé.

Point maintenu à part / non réalisé dans cette version :

- Autonomie complète vis-à-vis des dépôts GitHub/NuGet : intégrer les `.nupkg` locaux de toutes les dépendances NuGet/GitHub déjà utilisées par le projet, scanner compris, afin de permettre une restauration/compilation durablement offline sur un nouveau PC.

Contrôles :

- Contrôle statique effectué pour vérifier que l'appel `_scanButton.Clicked += ... ScanProductBarcodeAsync()` a bien une méthode correspondante dans `ProductCodeSelectionPage`.
- Contrôle statique effectué pour vérifier que la méthode utilise le service commun `ProductBarcodeScanService` et ne modifie pas la recherche article classique.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

État : réalisé dans la version 84.

---

## Version 85 — ajustement visuel des champs article avec scanner

Base de travail : `AGRA-EASY-MOBILE-Version84.zip`.

Correction intégrée :

- `ExpeditionFilterView`, `ReturnFilterView` et `CustomerBillingFilterView` : la zone de saisie article retrouve la hauteur utilisée avant l'ajout du scanner, comme en version 82.
- Le bouton de recherche article est réduit et reste sous le bouton scanner, sans agrandir le champ `Référence article` / `Article`.
- Le bouton scanner reste positionné au-dessus du bouton recherche, mais les deux boutons sont compactés pour tenir dans la hauteur existante du champ.
- `ProductCodeSelectionPage` : le bouton scanner de la zone de filtre article est réduit pour rester cohérent avec le design compact.
- Aucune modification fonctionnelle du scanner, de la recherche article classique ou de l'appel `FindProductCodeList(..., IsGenCode = true)`.

Documentation services web : pas de modification du document `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` pour cette version, car il s'agit uniquement d'un ajustement visuel.

Point maintenu à part / non réalisé dans cette version :

- Autonomie complète vis-à-vis des dépôts GitHub/NuGet : intégrer les `.nupkg` locaux de toutes les dépendances NuGet/GitHub déjà utilisées par le projet, scanner compris, afin de permettre une restauration/compilation durablement offline sur un nouveau PC.

Contrôles :

- Contrôle XML effectué sur les XAML modifiés.
- Contrôle statique effectué pour vérifier que les boutons scanner/recherche restent associés aux mêmes gestionnaires d'événements.
- Compilation réelle non exécutée dans l'environnement de génération, car `dotnet` n'est pas disponible.

État : réalisé dans la version 85.

---

# Version 86 — Corrections visuelles champs article/scanner et agencement des filtres

## Changements réalisés

- Conservation de la hauteur normale des champs de saisie article dans les vues d'édition des filtres.
- Réduction visuelle des actions article : scanner code-barres et recherche article, afin qu'elles restent plus compactes que l'icône calendrier.
- Remplacement des grands boutons article par de petites zones icône empilées, sans agrandir le champ `Référence article`.
- Correction appliquée à : `ExpeditionFilterView`, `ReturnFilterView`, `CustomerBillingFilterView` et `ProductCodeSelectionPage`.
- Repositionnement des zones `Code client` masquées selon profil vers la fin des grilles de filtres, en bas à droite lorsque cela s'applique, pour éviter un trou au milieu des vues quand elles sont invisibles pour les profils clients.
- Ajustement de `ShippingCostFilterView` pour placer le code client en bas à droite.
- Conservation des règles fonctionnelles existantes du scanner : appel `FindProductCodeList(..., IsGenCode = true)`, utilisation du premier article retourné si disponible, sinon du code scanné.
- Conservation de la recherche article classique.
- Rappel appliqué : le filtre frais de port ne force pas automatiquement `WithWaitingLines = true` lorsque les trois cases En attente / Facturé / Exonéré sont décochées.

## Documentation services web

Aucune modification de la documentation des services web n'a été nécessaire pour cette version, car les changements sont visuels et d'agencement, sans nouveau service, paramètre, propriété retournée ni nouvelle règle d'utilisation WebService.
---

## Version87 — accueil guide et correction définitive des champs article/scanner

Base de travail : `AGRA-EASY-MOBILE-Version86.zip`.

Modifications intégrées :

- Refonte de `MainPage` en page d'accueil condensée / menu guide : présentation courte AGRA EASY Mobile, bouton notifications en haut, raccourcis vers catalogue, panier, expéditions, retours, facturation et frais de port.
- Respect des droits existants : les raccourcis facturation/frais de port restent visibles uniquement selon `EasySession.IsCustomerBillingManager`.
- Correction visuelle des champs `Référence article` dans les vues de filtres : conservation de la hauteur normale du champ, colonne d'actions à droite, scanner en haut et recherche en bas, sans agrandir la zone.
- Repositionnement/homogénéisation des champs client dans les vues de filtre : le ou les champs client sont placés en fin de grille, avec bouton client aligné au format du bouton calendrier.
- Correction spécifique `CustomerBillingFilterView` : suppression du conflit d'emplacement dans la grille de critères et placement propre des zones client en bas.
- Ajustement visuel de `ProductCodeSelectionPage` pour conserver un bouton scanner compact intégré au champ.
- Aucune modification des appels WebService, de la logique de scanner ou de la recherche article classique.

Demande mise à côté / toujours en attente :

- Rendre le projet totalement autonome vis-à-vis des dépendances GitHub/NuGet avec `.nupkg` locaux et restauration hors ligne complète.

Documentation services web :

- Non modifiée pour cette version, car les changements sont uniquement visuels/navigation/agencement et ne changent aucun service, paramètre, classe retournée ou règle d'utilisation WebService.


---

# Demande planifiée — ajustements accueil, logo et navigation

## Constat

La page d'accueil V87 est globalement dans l'esprit demandé, mais plusieurs points visuels et de navigation doivent être corrigés avant validation.

## Corrections demandées

### Logo et identité visuelle

- Le logo AGRA/EASY de la page d'accueil est mal intégré avec l'arrière-plan actuel.
- Harmoniser le logo avec son fond : soit en adaptant l'arrière-plan à la couleur du logo, soit en retravaillant l'intégration du logo pour obtenir un rendu propre et cohérent.
- Appliquer le même principe au logo affiché dans le menu latéral.
- Ne pas dénaturer l'identité AGRA/EASY ; rester sobre et professionnel.

### Raccourci accueil

- Rendre le clic sur le logo du menu latéral utilisable comme raccourci vers la page d'accueil.
- Objectif : permettre à l'utilisateur de revenir facilement à l'accueil depuis n'importe quel module.

### Menu facturation / frais de port

- Revoir l'intégration du raccourci `Frais de port` : il ne doit pas être isolé de manière incohérente si sa place fonctionnelle est dans le périmètre facturation.
- Intégrer correctement ce raccourci dans la logique de menu facturation, sans casser l'accès direct utile depuis l'accueil.
- Respecter les droits et les règles de visibilité existants.

### Titre de la page d'accueil

- Le titre `Accueil` est jugé trop simpliste.
- Proposer un titre plus professionnel, plus représentatif de l'application et de son rôle de portail mobile, tout en restant court.

## Documentation services web

Aucune modification de la documentation des services web n'est nécessaire pour cette demande : il s'agit d'ajustements visuels, de navigation et d'organisation de menu, sans changement de service, paramètre, classe retournée ou règle d'appel WebService.

## État

Planifié pour la prochaine version, en attente de demande explicite de génération ZIP.

---

# Version 88 — Accueil, menu et image de démarrage

## Implémenté

- Refonte de la page d'accueil en mode plus condensé et mieux adapté à une application mobile professionnelle.
- Remplacement des raccourcis en grille par quatre raccourcis pleine largeur, un par ligne :
  - Catalogue et commandes ;
  - Expéditions ;
  - Retours et garanties ;
  - Facturations et frais de port.
- Ajout de descriptions courtes en face de chaque raccourci.
- Intégration de `Frais de port` dans le raccourci `Facturations et frais de port`, sans changer les routes existantes.
- Remplacement du titre simpliste `Accueil` par `EASY Mobile`.
- Harmonisation du logo AGRA sur la page d'accueil et dans le menu latéral via une version nettoyée avec fond transparent.
- Raccourci vers la page d'accueil depuis le logo du menu latéral.
- Création d'une image de démarrage à partir de l'image fournie : suppression visuelle du logo Sirius, conservation de la carte de France et du réseau DROP.
- Ajout d'un arrière-plan très léger issu de cette image sur la zone des raccourcis de l'accueil, sans forcer le rendu.

## Documentation services web

Aucune modification de documentation service web n'a été nécessaire : aucun service, paramètre, classe retournée ou règle d'appel WebService n'a changé.

---

# Version 89 - Corrections intégrées

## Accueil
- Ajout d'un espace visuel entre le logo AGRA et le titre `EASY Mobile` dans le cadre d'accueil.
- Remplacement des trois encadrés `Services / Logistique / EASY` par quatre encadrés représentant les entités principales :
  - Groupement AGRA ;
  - Réseau DROP ;
  - Réseau PROXIMICA ;
  - Réseau PPOINT-REPAR.
- Conservation du format mobile condensé de la page d'accueil.

## Champs article avec scanner
- Correction de l'agencement des boutons du champ article : la colonne droite est maintenant partagée équitablement entre le bouton scanner et le bouton recherche.
- Les deux boutons occupent chacun une moitié de la colonne, avec une petite marge et une symétrie visuelle.
- La taille globale du champ `Référence article` n'a pas été modifiée.
- La logique scanner/recherche et les appels WebService existants ne sont pas modifiés.

## Documentation services web
- Aucune modification de documentation service web n'est nécessaire pour cette version : les changements sont uniquement visuels/agencement.

---

# Version 90 — Alertes et corrections d'intégration

## Réalisé

- Remplacement de la référence service web par la nouvelle version fournie.
- Création du modèle `ShippingWarningFilter`.
- Création de `ShippingWarningFilterView` sans critère obligatoire, avec dates vides par défaut, filtre client selon profil, conteneur et plateforme via `GetWarehousesListV2`.
- Création de la vue liste `ShippingWarningListView`, utilisée à la place de l'ancienne vue de notifications.
- Le bouton cloche de l'accueil et celui du menu latéral ouvrent maintenant la liste des alertes.
- Chargement progressif des alertes avec `GetShippingWarningList`, `OnlyShortMessage = true`, `Offset` dynamique et `Count` fixé à 20.
- Conservation en mémoire de la liste pendant l'utilisation de la vue, anti-doublon par `ID` et tri décroissant par date.
- Marquage visuel des nouvelles alertes selon la date persistante `ShippingWarning.LastLaunchDate.{UserName}.{Warehouse}`.
- Contrôle périodique toutes les 10 minutes quand l'application est ouverte, avec changement d'icône si de nouvelles alertes existent.
- Création de `ShippingWarningDetailView`, affichage type mail, récupération par `GetShippingWarning`, prise de la première ligne si plusieurs sont retournées.
- Ajout d'une animation de chargement dans la vue des paramètres de connexion.
- Correction de la boucle d'annulation restante de `ShippingCostFilterView` lors de la première ouverture sans filtre existant.
- Agrandissement de l'image de démarrage DROP via le `BaseSize` du splash screen.
- Correction `AndroidStoreUncompressedFileExtensions` pour garder `resources.arsc` non compressé dans l'APK.
- Repositionnement du champ `Code client` de `ReturnFilterView` pour éviter le décalage visuel.
- Incrément de version Android : `ApplicationDisplayVersion = 1.90`, `ApplicationVersion = 90`.

## À garder en attente

- Intégration durablement offline des dépendances NuGet/GitHub via `.nupkg` locaux pour toutes les dépendances, à traiter séparément.
---

# Version 91 - Corrections intégrées

## Corrections intégrées

- Ajout de versions explicites communes dans `AGRA-EASY-MOBILE.csproj` : `Version=1.91`, `PackageVersion=1.91`, `ApplicationDisplayVersion=1.91`, `ApplicationVersion=91`.
- Conservation de la correction Android `.arsc` non compressé.
- Correction du contrôle périodique des alertes : si la vue des alertes n’a jamais été lancée et qu’aucune date persistante n’existe encore, toute alerte retournée par le service est considérée comme nouvelle.
- La surveillance automatique ne crée pas et ne met pas à jour la date persistante de dernier lancement. Cette date reste mise à jour uniquement à l’ouverture réelle de la vue des alertes.
- Le contrôle des alertes démarre immédiatement au lancement du Shell, puis continue toutes les 10 minutes.
- Ajout d’une animation visuelle sur les boutons Alertes de l’accueil et du menu latéral lorsque des alertes non visualisées existent.
- Confirmation de la correction `ShippingWarningFilterView.xaml.cs` avec usage de `FormatFilterDate(...)`.

## Documentation services web

- Documentation du scénario Alertes ajustée : absence de date persistante = toutes les alertes récupérées sont nouvelles.

## État

Livré dans la version 91.

---

# Version 92 — Corrections alertes et génération

## Corrections intégrées

- Correction du contrôle immédiat des nouvelles alertes après connexion et à l’affichage de l’accueil, pour éviter d’attendre le cycle périodique de 10 minutes.
- Conservation de la règle : la surveillance automatique ne crée pas la date de dernier lancement de la vue Alertes.
- Si la vue Alertes n’a jamais été ouverte et que la surveillance récupère au moins une alerte, les icônes Alertes doivent signaler des nouvelles alertes.
- Correction de `ShippingWarningFilterView.xaml.cs` : utilisation de `FormatFilterDate(...)` pour éviter l’erreur `ToString(...)`.
- Refonte compacte / Material Design de la liste des alertes.
- Suppression de l’affichage client et plateforme dans la liste des alertes.
- Refonte compacte / Material Design du détail d’alerte.
- Affichage discret du client dans le détail uniquement pour administrateur, au format `AccountCode - AccountName`, sans légende et sur une ligne séparée dans le corps du message.
- Masquage du client dans le détail pour les utilisateurs connectés de type client.
- Incrément version application à `1.92 / 92`.

## Documentation services web

Aucune modification de documentation services web : changements d’affichage, de version et de déclenchement de contrôle, sans changement de service, paramètre ou classe retournée.

---

# Version 93 — Livraison

## Corrections et évolutions appliquées

- Remplacement de l'identifiant applicatif par `fr.groupeagra.easymobile` dans `AGRA-EASY-MOBILE.csproj`.
- Passage de la version visible et package en `0.93` : `ApplicationDisplayVersion`, `Version` et `PackageVersion`.
- Passage du code version Android à `93` : `ApplicationVersion`.
- Correction complète de `ShippingWarningFilterView.xaml.cs` : suppression des appels `ToString("dd/MM/yyyy")` sur les dates sélectionnées et usage systématique de `FormatFilterDate(...)`.
- Reprise de l'image de démarrage : recadrage de la carte France / réseau DROP pour supprimer les marges inutiles et affichage interne de démarrage en plein écran via `StartupConnectionView` sans déformation.
- Conservation de la correction Android `.arsc` non compressé.

## Points d'attention

- Le splash natif Android peut rester brièvement limité par le système Android/MAUI, mais l'écran interne de démarrage affiche désormais l'image au maximum de l'écran sans déformation.
- Le changement d'identifiant applicatif fait considérer l'application comme une nouvelle application par Android/iOS.
- La compilation réelle n'a pas pu être exécutée dans l'environnement de génération si `dotnet` n'est pas disponible.


---

# Mise à jour documentation services web — fonctions supplémentaires panier/catalogue

## Documentation services web

- Ajout dans `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` des fonctions supplémentaires absentes de la documentation, sans duplication des fonctions déjà présentes.
- Scénario `Catalogue par référence article` : ajout de `GetAllConditionForArticleList` et `GetLocalConditionForArticleAndAlternative` comme fonctions supplémentaires du scénario.
- Scénario `Ajout au panier commande et commande externe` : ajout de `AddArticleToWarehouseShoppingCart` comme fonction supplémentaire du scénario.
- Scénario `Gestion du panier commande` : ajout de `GetShoppingCartWarehouseListV2`, `GetShoppingCartProductForWarehouseV2`, `ExistInShoppingCartForWarehouse`, `DeleteSorderLineByProductCode`, `DeleteSorderLineByProductCodeAndWarehouse` et `ClearShoppingCart` comme fonctions supplémentaires du scénario.
- Les fonctions déjà documentées n’ont pas été dupliquées : `GetLocalConditionForArticleList`, `GetAllConditionForArticleAndAlternative`, `GetAllConditionForArticle`, `GetLocalConditionForArticle`, `DeleteShoppingCartLine`, `GetShoppingCartDashboard` et `AddArticleListToWarehouseShoppingCart`.

## État

Mise à jour documentaire effectuée séparément, sans génération d’une nouvelle version ZIP projet.

---

# Mise à jour documentation services web — informations générales et sections administrateur

## Documentation services web

- Intégration en début de documentation des informations générales issues du manuel de passation de commande `ShoppingCartController` : objectif du service, adresses d’accès par dépôt, remarque sur l’interrogation d’une seule instance, authentification, compte de test, mécanisme de session/cookie, gestion des exceptions et terminologie des références article.
- Réorganisation documentaire des éléments réservés aux utilisateurs connectés de type `Administrateur` dans des sections dédiées `Spécificités administrateur` placées en fin des scénarios concernés.
- Déplacement ou isolement des services et usages administrateur suivants : `IsOrderManager`, `IsReturnSystemsSuperManager`, `SetOrderBasketAccountCode`, recherche catalogue par immatriculation, modification administrateur de date de livraison et numéro de lot, `UploadRefusedReturnPicture`, `SetReturnBasketAccountCode`.
- Ajout de précisions administrateur dans les scénarios de suivi lorsque `AccountCode` ou les informations client retournées sont à réserver au profil administrateur ou aux contextes habilités.
- Conservation des contrats de services et des règles métier existantes : les valeurs retournées par les services ne doivent pas être recalculées côté client.

## État

Mise à jour documentaire effectuée séparément, sans génération d’une nouvelle version ZIP projet.


---

## 2026-06-02 - Version de travail de nettoyage de la documentation services web

Demande : générer une version de documentation permettant de valider la vision de nettoyage, après remarque sur les rubriques remplies artificiellement et sur l'obligation de conserver tous les paramètres/propriétés connus.

Traitement réalisé :
- suppression ou allègement des points d'attention génériques, répétitifs ou orientés interface ;
- renommage de la rubrique « Règles transversales et pièges à retenir » en « Règles transversales importantes » ;
- conservation des signatures, paramètres et propriétés déjà documentés ;
- ajout d'une règle documentaire indiquant que les propriétés/paramètres connus doivent rester listés, même si non exploités par AGRA-EASY-MOBILE ;
- ajout des paramètres connus, extraits de la référence de service du projet, pour plusieurs fonctions ajoutées comme fonctions optionnelles/supplémentaires ;
- remplacement des mentions génériques trop orientées « mobile » par « client logiciel » ou « application cliente » quand le contrat de service web n'est pas propre au mobile.

Statut : version de travail pour validation utilisateur avant nettoyage définitif.


---

## 2026-06-02 - Précision documentaire sur les exceptions et IsConnected

Demande : clarifier la phrase indiquant qu'une exception ne doit pas être convertie en règle locale non validée, et ajouter la règle permettant de distinguer une exception métier d'une erreur de session ou de connexion.

Traitement réalisé :
- reformulation de la phrase concernée dans la section « Session, cookies et gestion des exceptions » ;
- ajout d'une règle indiquant qu'après réception d'une exception, le client logiciel doit contrôler la session avec `IsConnected` ;
- précision : si `IsConnected` confirme que la session est valide, l'exception peut être traitée comme une exception de logique métier retournée par le service ;
- précision : si `IsConnected` indique que la session n'est plus valide, l'erreur doit être traitée comme un problème de connexion/session et non comme une règle métier.

Statut : mise à jour documentaire effectuée séparément, sans génération d'un ZIP global du projet.


---

## 2026-06-02 - Ajustement rédactionnel : guide factuel d'utilisation du service web

Demande : limiter la documentation à un guide d'utilisation du service web destiné aux développeurs consommateurs, avec des faits, règles, descriptions et conseils pratiques, sans formulations à interpréter.

Traitement réalisé :
- reformulation de l'introduction pour préciser que le document est un guide d'utilisation du service web `ShoppingCartController` pour développeurs consommateurs ;
- remplacement de la phrase sur les exceptions afin de supprimer la notion de « règle locale non validée » ;
- conservation de la règle pratique `IsConnected` après réception d'une exception pour distinguer une réponse métier du service d'une session expirée ou déconnectée ;
- remplacement d'un point d'attention ambigu sur `GetExternalStockStatus` par une formulation factuelle avec description des valeurs à compléter ;
- remplacement de la formulation « doit être interprétée » pour `SupplierResponse = En attente` par une règle factuelle indiquant l'absence de décision PDF finalisée.

Statut : mise à jour documentaire effectuée séparément, sans génération d'un ZIP global du projet.

## Mise à jour documentation - section 3.2 Droits fonctionnels et blocages métier

- Ajout d'une précision dans la section `3.2. Droits fonctionnels et blocages métier`.
- Les fonctions de droits et de blocages sont décrites comme des fonctions d'anticipation des rejets possibles des services métiers appelés.
- La documentation précise que ces fonctions permettent d'éviter ou de préparer les exceptions liées à un manque d'autorisation ou à un contexte non favorable.
- La documentation précise que ces fonctions ne servent pas à forcer une demande ni à contourner un contrôle du service web.
- L'appel à ces fonctions reste optionnel côté intégrateur, mais l'application cliente doit alors gérer les exceptions retournées directement par les services métiers appelés.
- Aucun ZIP global du projet n'a été généré.


---

## Mise à jour documentation - restructuration par scénarios et cas clients dépôt

Date : 2026-06-03

Demande traitée :
- Restructurer le corps de la documentation comme un guide par scénarios réels d’utilisation, et non comme un catalogue de fonctions.
- Garder un sommaire intuitif basé sur les scénarios : recherche de pièces, consultation conditions, ajout panier, validation commande, suivis, retours, factures, alertes.
- Replacer les fonctions de droits/blocages dans les scénarios où elles sont utiles, avec renvoi implicite par scénario et sans section autonome détaillée au début du document.
- Placer les spécificités administrateur / applications internes en fin de scénario, comme informations secondaires destinées à la complétude et au rappel interne.
- Placer les fonctions demandées mais non principales dans des rubriques “fonctions supplémentaires / optionnelles” ou dans le scénario le plus cohérent.
- Expliciter l’enchaînement principal de recherche de pièces : FindArticleList -> construction GenericArticle[] à partir de ShortProductCode, SupplierCode, Catalog -> GetAllConditionForArticleList / GetLocalConditionForArticleList.
- Ajouter un traitement documentaire spécifique au cas client dépôt identifié par GetDepotSupplierCode(AccountCode), en précisant qu’il s’agit d’un cas minoritaire et non du parcours standard.

Livrables :
- Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_MAJ_restructuration_clients_depots.docx
- Suivi_AGRA_EASY_MOBILE_demandes_MAJ_restructuration_clients_depots.md
- AGRA_EASY_MOBILE_documentation_et_suivi_MAJ_restructuration_clients_depots.zip

ZIP global projet : non généré.


## 2026-06-03 — Règle de livraison documentaire versionnée

Demande utilisateur : à partir de maintenant, fournir le fichier de documentation avec un numéro de version, comme pour les ZIP de projet.

Règle appliquée : les prochaines livraisons documentaires devront intégrer le numéro de version applicative dans le nom du fichier de documentation et dans le nom du ZIP séparé documentation + suivi.

Version concernée pour la livraison actuelle : Version 93 / 0.93.


---

## 2026-06-07 — Relecture utilisateur de la documentation Version 93

Demande utilisateur : prendre comme base le document retravaillé par l'utilisateur, corriger uniquement la langue et la grammaire sans modifier le contenu métier sans validation explicite, rendre le sommaire cliquable, compléter la carte des scénarios, remplacer les retours `void` par une formulation plus lisible, ajuster la largeur de la colonne `Type`, comparer avec la version précédente et prévoir l'indication des plages de valeurs pour les filtres lorsque ces valeurs sont connues.

Traitement réalisé :
- document utilisateur conservé comme base de travail ;
- corrections grammaticales et orthographiques ciblées, sans réécriture métier volontaire ;
- sommaire reconstruit avec des liens internes cliquables vers les titres du document ;
- carte des scénarios enrichie avec les fonctions documentées par scénario ;
- remplacement des retours `void` par `aucun retour prévu (void)` dans les lignes de synthèse des fonctions ;
- ajustement de la largeur des colonnes des tableaux à trois colonnes pour réduire la colonne `Type` et redonner de l'espace à la colonne `Description` ;
- ajout d'une règle documentaire sur les plages/listes de valeurs des filtres : à renseigner uniquement quand elles sont connues, sans ajout automatique d'une mention pour les valeurs inconnues ;
- aucune génération de ZIP global du projet.

Réserves/propositions à valider :
- certaines rubriques contiennent encore des doublons ou des formulations techniques longues issues des versions précédentes ; elles n'ont pas été supprimées automatiquement afin de respecter la consigne de ne pas modifier le contenu sans validation explicite ;
- les plages de valeurs des filtres ne doivent pas être inventées : elles doivent être complétées seulement lorsqu'elles sont confirmées par le service ou par validation métier.


---

## 2026-06-07 — Correction ciblée après validation utilisateur : suppression de la mention automatique sur les valeurs possibles

Demande utilisateur : supprimer la mention ajoutée automatiquement `Valeurs possibles : à compléter` et conserver la carte des scénarios lorsque celle-ci reste présentée sous forme de tableau bien complété.

Traitement réalisé :
- suppression de la mention automatique `Valeurs possibles : à compléter` dans la documentation ;
- conservation de la carte des scénarios d'utilisation, sans modification de contenu ;
- aucune modification métier supplémentaire ;
- aucun ZIP global du projet généré.

---

# Mise à jour documentaire Version 93 — filtres à valeurs fixes et diagnostic alertes

## Documentation services web

- Mise à jour de `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios.docx` à partir de la dernière version validée.
- Ajout uniquement des plages/listes de valeurs validées par le demandeur :
  - filtre `SorderType` pour les scénarios de suivi commandes / ruptures / bons de livraison : `STOCK`, `MAGASIN`, `EXPRESS`, `EXTERNE` ; valeur vide ou non renseignée = critère non appliqué ;
  - filtres `ReturnStatus` et `ProductStatus` pour `GetReturnsLines` :
    - `ReturnStatus` : `treated` = Traité, `waiting` = En cours, `external` = Externe ;
    - `ProductStatus` : `accepted` = Accepté, `warranty` = Accepté en garantie, `refunded` = Remboursé, `supplier` = Retour au fournisseur, `refused` = Rejeté, `missing` = Manquant ;
    - valeur vide ou non renseignée = critère non appliqué ;
  - filtre `DeliveryType` pour `GetInvoiceWaitingLines` : `DEPOT`, `EXPRESS`, `MAGASIN`, `STOCK`, `IMPLANTATION`, `REFACTURATION`, `DROP-SHIPPING`, `PORT`, `MANUEL`, `REGULARISATION`, `PSD`, `PERIODIC`.
- Aucun ajout sur les filtres frais de port, remboursements fournisseurs ou alertes : ces points avaient été considérés comme déjà suffisamment expliqués dans la documentation.

## Diagnostic statique — vue Alertes

- Analyse statique effectuée sur `Views/ShippingWarningListView.xaml`, `Views/ShippingWarningListView.xaml.cs`, `Services/EasySession.cs`, `Models/ShippingWarningFilter.cs` et le suivi projet.
- Règle attendue rappelée dans le suivi projet : chargement progressif des alertes avec `GetShippingWarningList`, `OnlyShortMessage = true`, `Offset` dynamique et `Count = 20`.
- Constat dans `EasySession.GetShippingWarningListAsync(...)` : l'appel transmet bien `OnlyShortMessage = true`, `Offset` et `Count` au service.
- Constat dans `ShippingWarningListView` : le premier chargement appelle `LoadNextPageAsync(false)` avec `Offset = 0` et `Count = 20` ; le chargement suivant utilise `Offset = Warnings.Count` et `Count = 20`.
- Point d'anomalie identifié : si un appel de pagination retourne 0 ligne, `LoadNextPageAsync(...)` effectue un nouvel appel avec `Offset = null`, ce qui peut relire la première page au lieu de considérer que la fin de liste est atteinte. Cette logique est particulièrement problématique en mode ajout (`append = true`).
- Point d'anomalie identifié : aucun indicateur de fin de liste (`hasMore` / `noMoreItems`) n'est mémorisé. Tant que l'utilisateur reste proche de la fin de liste, l'événement de seuil peut relancer des appels même si aucune page suivante n'existe.
- Conclusion statique : la règle de base `Offset` dynamique / `Count = 20` est présente, mais l'implémentation n'est pas assez protégée contre la relecture de la première page et les appels répétés sans résultat en fin de liste.

## À valider avant correction applicative

- Correction proposée : supprimer le repli `Offset = null` lors des chargements suivants (`append = true`).
- Correction proposée : ajouter un indicateur `_hasMoreWarnings` initialisé à `true`, remis à zéro lors d'un filtre/reset, et passé à `false` lorsque le service retourne moins de 20 lignes ou 0 ligne.
- Correction proposée : ne déclencher un chargement suivant que si `_hasMoreWarnings == true` et si aucun chargement n'est en cours.
- Correction proposée : conserver le premier chargement à `Offset = 0` et `Count = 20`, sans changer les paramètres du service.
- Aucune modification du code applicatif n'a été intégrée dans cette livraison documentaire.

---

## Mise à jour documentation Version93 - retrait de la section de consignes rédactionnelles

Date : 2026-06-07

Demande utilisateur :
- Prendre comme base le document Word retravaillé et fourni par l'utilisateur.
- Ne pas intégrer la rubrique « 6. Règles transversales importantes » dans la documentation finale, car il s'agit de consignes de rédaction destinées à l'assistant, et non d'un contenu du guide développeur.
- Ne plus modifier le contenu de cette version de documentation sans demande explicite.

Traitement effectué :
- Suppression de l'entrée résiduelle « 6. Règles transversales importantes » dans le sommaire.
- Aucune modification métier volontaire du contenu.
- Conservation de la structure et du contenu du document fourni par l'utilisateur.
- Génération d'une documentation versionnée Version93.

Livrables :
- Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version93_MAJ_sans_regles_transversales.docx
- Suivi_AGRA_EASY_MOBILE_demandes_Version93_MAJ_sans_regles_transversales.md
- AGRA_EASY_MOBILE_Version93_documentation_et_suivi_MAJ_sans_regles_transversales.zip

---

# Version 94 — Mise à jour des références du service web

Date : 2026-06-08

## Demande utilisateur

- Mettre à jour le projet avec les deux fichiers fournis après régénération des références du service web :
  - `ConnectedService(1).json` ;
  - `Reference(5).cs`.
- Les références de service web ont été mises à jour côté utilisateur après suppression de certaines propriétés de classes de retour.

## Modifications intégrées

- Remplacement du fichier `Connected Services/Services/ConnectedService.json` par le fichier fourni.
- Remplacement du fichier généré `Connected Services/Services/Reference.cs` par le fichier fourni.
- Analyse comparative statique entre l'ancienne et la nouvelle référence de service : la classe `SorderBasketLine` ne contient plus les propriétés suivantes :
  - `Price` ;
  - `Discount` ;
  - `NewPrice` ;
  - `NewDiscount` ;
  - `AdditionalDiscount` ;
  - `IsNet` ;
  - `GarageProductPrice` ;
  - `GarageProductDiscount`.
- Correction ciblée dans `Views/OrderBasketView.cs` pour remplacer l'affichage de `line.Price` et `line.Discount` par les propriétés encore présentes dans la référence générée :
  - `line.ProductPrice` ;
  - `line.ProductDiscount`.
- Incrément de version application à `0.94 / 94` dans `AGRA-EASY-MOBILE.csproj` :
  - `ApplicationDisplayVersion = 0.94` ;
  - `ApplicationVersion = 94` ;
  - `Version = 0.94` ;
  - `PackageVersion = 0.94`.
- Intégration de la documentation services web courante dans le projet avec un nom de fichier versionné :
  - `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version94.docx`.

## Vérifications effectuées

- Recherche statique des propriétés supprimées dans le code applicatif.
- Correction du seul usage direct bloquant identifié dans la vue panier de commande.
- Vérification textuelle que les propriétés supprimées de `SorderBasketLine` ne sont plus appelées dans `OrderBasketView.cs`.

## Limites de vérification

- La compilation réelle n'a pas pu être exécutée dans cet environnement car la commande `dotnet` n'est pas disponible.
- La validation finale de compilation doit être effectuée dans Visual Studio ou dans un environnement .NET MAUI disposant du SDK requis.

## Livrables

- Projet complet : `AGRA-EASY-MOBILE-Version94.zip`.
- ZIP séparé documentation + suivi : `AGRA_EASY_MOBILE_Version94_documentation_et_suivi_MAJ_references_service_web.zip`.


---

# Version 95 — Correction du maintien de position dans la liste des alertes

Date : 2026-06-08

## Demande utilisateur

- Corriger le comportement de la vue Alertes après la récupération d'une nouvelle page.
- La liste ne doit pas revenir systématiquement au début après un chargement progressif.
- Le retour automatique au début doit se produire uniquement si des alertes plus récentes que l'élément le plus récent déjà chargé sont ajoutées à la liste.

## Modifications intégrées

- Correction ciblée dans `Views/ShippingWarningListView.xaml.cs`.
- Suppression du rafraîchissement global de la collection par `Warnings.Clear()` puis réinsertion complète après chaque chargement.
- Ajout d'une insertion progressive des nouvelles alertes dans `ObservableCollection<ShippingWarningListItem>` en conservant l'ordre décroissant par `CreationDate`.
- Conservation de la position de scroll lors du chargement des pages suivantes lorsque les alertes ajoutées sont plus anciennes que la première alerte déjà affichée.
- Conservation du retour automatique au début uniquement lorsqu'une alerte ajoutée possède une `CreationDate` strictement supérieure à la date la plus récente déjà présente dans la liste avant l'appel.
- Ajout de l'indicateur `_hasMoreWarnings` pour éviter de relancer des appels de pagination après une page vide ou incomplète.
- Réinitialisation de `_hasMoreWarnings` lors d'un reset de liste ou d'un nouveau chargement initial.
- Le chargement progressif conserve `Offset = Warnings.Count` et `Count = 20`.

## Fichiers modifiés

- `Views/ShippingWarningListView.xaml.cs`
- `AGRA-EASY-MOBILE.csproj`
- `Suivi_AGRA_EASY_MOBILE_demandes.md`
- `REPRISE_NOUVELLE_DISCUSSION_AGRA_EASY_MOBILE.md`
- `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version96.docx` : copie versionnée de la documentation courante.

## Version

- `ApplicationDisplayVersion = 0.95`
- `ApplicationVersion = 95`
- `Version = 0.95`
- `PackageVersion = 0.95`

## Vérifications effectuées

- Vérification statique de la vue `ShippingWarningListView`.
- Vérification que le chargement suivant est ignoré si `_hasMoreWarnings == false`.
- Vérification que la collection n'est plus vidée/reconstruite à chaque page récupérée.
- Vérification que le retour au début dépend uniquement de l'ajout d'une alerte plus récente que la plus récente déjà chargée.

## Limites de vérification

- Compilation réelle non exécutée dans l'environnement de génération car la commande `dotnet` n'est pas disponible.
- Test fonctionnel à réaliser sur appareil/émulateur MAUI.

# Version 96 — Correction du chargement progressif des alertes

## Demande
- Après la correction de la pagination des alertes, la liste ne charge plus que la première vingtaine de lignes et ne récupère plus de page suivante au défilement.
- Le comportement attendu est le suivant :
  - chargement initial de la première page ;
  - chargement des pages suivantes uniquement lors d'un défilement vers le bas proche de la fin de liste ;
  - aucun chargement déclenché lors d'un défilement vers le haut ;
  - maintien de la position courante après ajout d'alertes plus anciennes ;
  - retour au début uniquement si une récupération ajoute des alertes plus récentes que l'alerte la plus récente déjà présente.

## Diagnostic
- La version 95 arrêtait la pagination dès qu'une page retournait moins de `PageSize` lignes.
- Ce comportement pouvait bloquer le chargement des pages suivantes si le service retournait une page légèrement inférieure à 20 lignes, notamment à cause de la requête SQL de pagination côté service.
- Le déclenchement `RemainingItemsThresholdReached` de `CollectionView` n'était pas assez fiable pour couvrir correctement ce scénario et pouvait ne pas se redéclencher après le premier chargement.

## Correction appliquée
- Remplacement du déclenchement `RemainingItemsThresholdReached` par l'événement `Scrolled` de la `CollectionView`.
- Chargement de la page suivante uniquement si `VerticalDelta > 0` et si le dernier élément visible est proche de la fin de la liste.
- Conservation de la position courante après ajout d'alertes plus anciennes.
- `_hasMoreWarnings` n'est plus mis à `false` lorsqu'une page contient moins de 20 lignes ; l'arrêt définitif est déclenché uniquement quand le service retourne 0 ligne.
- Conservation de la logique de retour au début uniquement lorsqu'une alerte ajoutée est plus récente que l'alerte la plus récente déjà présente.

## Fichiers modifiés
- `Views/ShippingWarningListView.xaml`
- `Views/ShippingWarningListView.xaml.cs`
- `AGRA-EASY-MOBILE.csproj`
- `Suivi_AGRA_EASY_MOBILE_demandes.md`
- `REPRISE_NOUVELLE_DISCUSSION_AGRA_EASY_MOBILE.md`

## Version
- `ApplicationDisplayVersion = 0.96`
- `ApplicationVersion = 96`
- `Version = 0.96`
- `PackageVersion = 0.96`

## Vérification
- Vérification statique du code effectuée.
- Compilation non effectuée dans l'environnement actuel : outil `dotnet` indisponible.
