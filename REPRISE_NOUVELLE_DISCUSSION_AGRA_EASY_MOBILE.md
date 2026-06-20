# Reprise nouvelle discussion - AGRA-EASY-MOBILE

## A lire en premier

Ce fichier sert de point d'entrée pour reprendre le développement dans une nouvelle discussion. La source de vérité est le dossier projet contenu dans ce ZIP, et en particulier :

- `Suivi_AGRA_EASY_MOBILE_demandes.md`
- `Transfert_AGRA_EASY_MOBILE_CDC_regles.docx`
- `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version94.docx`

Avant toute réponse technique, modification de code ou génération de version, il faut lire ces fichiers.

## Base de travail actuelle

La base de travail officielle à reprendre est ce ZIP : `AGRA-EASY-MOBILE-Version94.zip`.

Cette version a été générée à partir de `AGRA-EASY-MOBILE-Version93.zip`, avec mise à jour des références générées du service web.

Les versions générées précédemment depuis une base plus ancienne, notamment la version de transmission produite à tort depuis Version67/Version68, ne doivent pas être utilisées comme base de développement.

## Etat fonctionnel courant à retenir

Le projet contient notamment les évolutions suivantes :

- `CustomerBillingListView`, `CustomerBillingFilter` et `CustomerBillingFilterView` pour le suivi des factures client.
- Référence service web corrigée avec `GetCustomerBillingLines(string CustomerBillingId, string Warehouse)`.
- Gestion des factures client avec `CustomerBillingId` comme clé métier principale.
- Description métier de `CustomerBillingLine` intégrée aux documents projet : distinction entre entête facture, détail facture, et totaux dont le sens dépend du contexte.
- Droit de facturation via `IsCustomerBillingManager` récupéré après connexion et pris en compte avant construction du Shell.
- Démarrage hors `AppShell` via `StartupConnectionView`, puis création du Shell après connexion réussie et récupération des droits.
- Reconnexion automatique après perte de session conservée.
- Suivi BL avec raccourci vers panier retour et ajout d'une ligne de BL au panier retour selon les règles validées.
- `ReturnBasketView` affiche la section client affecté pour Administrateur, avec modification autorisée seulement si le panier retour est vide.

- `InvoiceWaitingListView` pour le suivi de la facturation en attente, basé sur `ExpeditionFilterView` en mode `Type de livraison` et sur le service `GetInvoiceWaitingLines`.
- `ExpeditionFilter` contient `DeliveryType` pour le filtre de facturation en attente, en complément de `SorderType` pour les modules d’expédition classiques.
- `ContainerNo` saisi dans le filtre expédition est normalisé à 18 caractères par complétion de zéros à gauche lorsqu’il est non vide et fait 18 caractères ou moins ; au-delà de 18 caractères, il reste inchangé.
- `ExpeditionFilterView` utilise deux pickers statiques indépendants pour éviter de mélanger `ItemsSource` XAML et manipulation dynamique : `PickerSorderType` pour les types de commande et `PickerDeliveryType` pour les types de livraison du module facturation en attente.
- `RefundInvoiceListView` remplace le placeholder de l’onglet `À rembourser` et affiche le suivi des remboursements en attente via `GetRefundWaitingLines`. Le type généré à conserver est `RefundWatingLine`.
- `ReturnBasketView` affiche désormais la plateforme au milieu de la ligne contenant le numéro de BL.
- `RefusedReturnListView` affiche un bouton photo réservé aux administrateurs après la plateforme ; ce bouton ouvre une activité caméra Android plein écran dédiée, sans boîte de choix intermédiaire, avec une action de capture et une action de sélection de photo existante.
- La photo de retour refusé est transmise via `UploadRefusedReturnPicture` après conversion/garantie JPG ; aucune dépendance externe caméra n’a été ajoutée.

## Demandes fonctionnelles actives en attente

A la génération de cette version de transmission, aucune demande fonctionnelle active ne doit être générée automatiquement dans la prochaine version.

Toute nouvelle correction ou évolution doit faire l'objet d'une validation explicite, ou d'un CDC si elle est importante.

## Demandes mises de côté jusqu'à nouvel ordre

### Facade REST/JSON à partir du SOAP existant

Demande à ne pas intégrer dans la prochaine version sans ordre explicite : préparer plus tard une facade REST/JSON équivalente uniquement aux services web réellement utilisés par l'application mobile AGRA-EASY-MOBILE.

Périmètre strict :

- uniquement les scénarios et services réellement utilisés par l'application mobile ;
- pas toute la référence SOAP ;
- pas tout l'ASMX ;
- pas les fonctions non consommées par l'application ;
- conserver les signatures, paramètres, structures JSON et contenus échangés de manière stable pour préparer une future migration serveur vers ASP.NET moderne / .NET 10 ;
- cette demande doit être reprise uniquement si l'utilisateur fournit le code ASMX et demande explicitement ce chantier.

## Règles permanentes de développement

1. Les fichiers du projet fourni sont la source de vérité.
2. Lire les fichiers de suivi/transfert/documentation avant de répondre ou modifier le code.
3. Ne pas improviser de service web, propriété, règle métier, fallback, mapping ou comportement non présent dans les fichiers ou non validé explicitement.
4. Ne pas modifier un programme, une vue, une référence générée, une logique de connexion/reconnexion, un wrapper existant ou une logique transverse sensible sans demande explicite.
5. Appliquer des modifications minimales et ciblées.
6. Préserver les règles métier et comportements existants, notamment hérités des écrans ASPX/WebForms.
7. Pour les évolutions importantes, produire un CDC détaillé avant implémentation.
8. Signaler toute ambiguïté, API absente, propriété absente, signature différente, fichier manquant ou règle métier incertaine.
9. Ne pas générer de ZIP ou incrémenter la version sans demande explicite.
10. Mettre à jour les fichiers de suivi projet après chaque évolution/génération.
11. Ne pas stocker les demandes détaillées du projet en mémoire ; les conserver dans les fichiers du projet.
12. Maintenir la documentation service web comme une documentation actuelle et homogène, organisée par scénarios réels de l'application, sans chronologie de versions ni documentation exhaustive de tout le SOAP.
13. Penser multi-plateforme MAUI : Android, Windows, iOS et autres cibles possibles.
14. Respecter le style visuel existant.
15. Ne jamais recalculer ni réinterpréter les valeurs métier retournées par les services web sauf règle explicite.
16. Dire clairement si la compilation, l'exécution ou les tests réels ne sont pas possibles.
17. Si l'utilisateur fournit une nouvelle version ZIP, cette version devient la nouvelle base opérationnelle.
18. Ne jamais fournir de fichiers sources partiels, patchs, extraits de code ou correctifs isolés comme livrables téléchargeables ; livrer uniquement un ZIP complet du projet quand l'utilisateur le demande explicitement.
19. Pour chaque demande planifiée ou en attente, fournir systématiquement un fichier de suivi téléchargeable séparé et intégrer ce même fichier dans le dossier projet.
20. Le fichier généré `Connected Services/Services/Reference.cs` reste la base de référence obligatoire pour les signatures, types, classes, propriétés et paramètres du service web SOAP. Même si le wrapper applicatif `EasySession` est réorganisé, séparé par plateforme ou implémenté manuellement pour contourner une contrainte technique iOS/Android/Windows, il doit toujours être construit à partir des contrats présents dans `Reference.cs` et ne jamais inventer de signature, mapping ou type hors de cette référence. Après chaque régénération de `Reference.cs` côté serveur, les wrappers `EasySession` doivent être réalignés sur ce fichier généré.
21. La classe SOAP manuelle utilisée pour iOS, notamment `Services/EasySoapManualClient.cs` et son intégration dans `Services/EasySession.cs`, doit toujours rester strictement alignée sur `Connected Services/Services/Reference.cs`. Après chaque régénération de `Reference.cs`, vérifier et réaligner les noms d'opérations, noms de paramètres, types retour, tableaux, objets complexes et ordre des propriétés XML utilisés par le SOAP manuel iOS. Le client manuel iOS est un contournement technique de la contrainte AOT/Reflection.Emit, pas une source de contrat indépendante.

## Documentation à maintenir

Avant chaque génération de nouvelle version ZIP :

- mettre à jour `Suivi_AGRA_EASY_MOBILE_demandes.md` ;
- mettre à jour `Transfert_AGRA_EASY_MOBILE_CDC_regles.docx` ;
- mettre à jour `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version94.docx` si des scénarios, services, paramètres, classes, propriétés ou règles métier de services web changent ;
- intégrer ces documents dans le ZIP final.

## Limites connues de l'environnement de génération

Dans l'environnement ChatGPT utilisé précédemment, la compilation réelle n'a pas pu être exécutée car `dotnet` n'était pas disponible. Les contrôles effectués sont donc des contrôles statiques, vérifications de fichiers, XAML/XML, intégrité ZIP et rendu des documents Word en PNG.

## Dernière base générée

Dernière version générée dans cette discussion : `AGRA-EASY-MOBILE-Version94.zip`.

Base précédente : `AGRA-EASY-MOBILE-Version93.zip`.

Principales évolutions de la version 75 :

- Correction d'affichage du module `Facturation en attente` : préfixe `N°`, tri par défaut par date de création décroissante, libellé `Prix BL :`, répartition dynamique des lignes d'entête et de détail, préfixe `Cmd :`.
- Correction du panier de retour : affichage de la plateforme au milieu de la ligne contenant le numéro de BL.
- Création du module `Remboursements en attente` sur l'onglet `À rembourser`, basé sur le filtre retour avec états masqués et sur le service `GetRefundWaitingLines`.
- Vérification effectuée dans la référence : le type retourné par le service est `RefundWatingLine`, orthographe à conserver car elle provient du fichier généré `Reference.cs`.

Règle de livraison à respecter : ne jamais fournir de fichiers sources partiels, patchs ou extraits téléchargeables. Sur demande explicite d'implémentation, générer uniquement un ZIP complet avec numéro de version incrémenté, et fournir séparément le fichier de suivi des demandes, également intégré au projet.


## Principales évolutions de la version 76

- Corrections des modules `Facturation en attente` et `Remboursements en attente` : masquage des zones à zéro, préfixe `BL :`, alignement BL/TVA, et dépréciations affichées en pourcentage entier.
- Création du module `Frais de port`, accessible dans la zone facturation à la place de l'ancienne entrée `Refacturation`.
- Ajout de `ShippingCostFilter`, `ShippingCostFilterView`, `ShippingCostListView` et du wrapper `EasySession.GetShippingCostLinesAsync`.
- Le filtre frais de port impose au moins l'un des critères `AccountCode`, `ShippingCostDeliveryNumber` ou `TurnoverDeliveryNumber`.
- La liste des plateformes du filtre frais de port vient de `GetWarehousesListV2`, affiche `WarehouseCode`, et exclut les plateformes externes (`IsExternal = true`).
- Le filtre `DeliveryType` des frais de port a été abandonné pour la version mobile et ne doit pas être réintroduit sans demande explicite.

## Principales évolutions de la version 77

- Ajustement de la carte `Frais de port` : bouton `Lignes` replacé à droite du bloc adresse, sur la hauteur des deux lignes d'adresse.
- Ajustement de la deuxième ligne d'entête `Frais de port` : zone poids total / nombre de colis centrée dynamiquement, avec priorité d'espace au nom du transporteur si nécessaire.
- Ajustement du filtre `Frais de port` : dates proposées par défaut du 1er au 7 sur le mois précédent + mois courant, puis du 8 à la fin du mois sur le mois courant uniquement.
- Ajustement de la ligne de détail `Frais de port` : préfixe `Colis : ` devant le numéro de colis, sans décaler la zone `BL : ...`.
## Principales évolutions de la version 78

- Correction de l'annulation des filtres `Factures client` et `Frais de port` : si le filtre a été ouvert automatiquement en l'absence de filtre administrateur valide, le bouton `Annuler` revient à l'accueil pour éviter la boucle d'ouverture automatique.
- Ajustement des cartes `Frais de port` : suppression du bouton `Lignes`; l'affichage des détails reste piloté uniquement par la case globale `Détail`.
- Ajustement des cartes `Frais de port` : le code client est affiché sur la ligne ville/code postal, aligné à droite, uniquement pour les utilisateurs administrateurs.

## Principales évolutions de la version 79

- Module `Frais de port` : suppression de la règle non demandée qui imposait au moins une case cochée parmi `En attente`, `Facturé` et `Exonéré`. Les trois booléens sont transmis tels quels au service.
- Module `Frais de port` : description affichée sur une seule ligne, avec zone toujours réservée.
- Module `Retours refusés` : bouton photo administrateur ajouté après la plateforme. La photo est prise ou sélectionnée via les capacités natives MAUI/Android, convertie en JPG côté Android, puis envoyée avec `UploadRefusedReturnPicture`.
- Aucune dépendance GitHub n’a été ajoutée sans validation explicite.

## Principales évolutions de la version 80

- Module `Retours refusés` : remplacement du flux `MediaPicker` par une vraie interface caméra Android plein écran dédiée.
- Le clic administrateur sur le bouton photo ouvre directement l’aperçu vidéo, avec seulement deux actions principales : prendre une photo ou choisir une photo existante.
- La permission caméra est demandée uniquement au déclenchement de cette fonction par un administrateur ; elle n’est pas demandée au lancement de l’application ni aux utilisateurs non administrateurs.
- La permission `READ_MEDIA_IMAGES` ajoutée précédemment a été retirée ; la sélection utilise le sélecteur Android avec accès ponctuel au fichier choisi.
- Aucune dépendance GitHub/NuGet externe n’a été ajoutée.


## Principales évolutions de la version 81

- Correction des erreurs de compilation Android dans `Platforms/Android/RefusedReturnCameraActivity.cs` liées à l’interface caméra plein écran ajoutée en V80.
- Levée des ambiguïtés de noms `Camera`, `ImageButton` et `Path` par qualification explicite des types Android et `System.IO`.
- Correction de l’affectation invalide sur `LinearLayout.Gravity` par l’appel Android approprié.
- Conservation du comportement métier : fonction photo réservée aux administrateurs, permission caméra demandée uniquement au clic, interface Android plein écran avec prise de photo ou sélection d’une photo existante, conversion JPG et envoi via `UploadRefusedReturnPicture`.
- Aucune dépendance externe n’a été ajoutée.

## Principales évolutions de la version 82

- Correction des dernières erreurs de compilation Android dans `Platforms/Android/RefusedReturnCameraActivity.cs` : suppression de l'appel invalide `SetJpegQuality(90)` et application de `ImageView.ScaleType.Center` via `button.SetScaleType(...)`.
- Module `Retours refusés` : le bouton photo administrateur n'est visible/accessible que lorsque la plateforme de la ligne correspond à la plateforme courante de connexion.
- Filtres `ExpeditionFilter`, `ReturnFilter`, `CustomerBillingFilter` et `ShippingCostFilter` : les dates par défaut sont appliquées uniquement à la création initiale du filtre. Si l'utilisateur efface les dates dans un filtre existant, elles ne sont plus restaurées automatiquement.
- `CustomerBillingFilter` : dates par défaut à la création initiale uniquement = premier jour du mois précédent jusqu'à la date du jour.
- Nettoyage documentaire du dossier projet : conservation d'un seul fichier de suivi stable et d'un seul fichier de reprise stable, sans doublons nommés par version.


## Principales évolutions de la version 83

- Scanner code-barres article intégré avec `BarcodeScanning.Native.Maui` 3.0.3 par référence NuGet classique.
- `BarcodeScannerPage` retourne automatiquement la valeur détectée.
- `ProductBarcodeScanService` résout le code scanné via `FindProductCodeList(..., IsGenCode=true)`.
- Boutons scanner ajoutés aux saisies article des filtres expédition, retour, factures client et à `ProductCodeSelectionPage`.
- La documentation des services web a été mise à jour pour le scénario code-barres de `FindProductCodeList`.

## Principales évolutions de la version 84

- Correction de `ProductCodeSelectionPage.cs` : ajout de la méthode manquante `ScanProductBarcodeAsync()` appelée par le bouton scanner.
- La page de sélection article peut maintenant appeler `ProductBarcodeScanService.ScanAndResolveProductAsync(this)` et retourner directement l'article résolu par scan.
- Recherche article classique et filtres existants non modifiés.

## Demande maintenue à part — autonomie NuGet/GitHub complète

- Le projet doit à terme intégrer localement les `.nupkg` de toutes les dépendances NuGet/GitHub déjà utilisées, dépendances transitives comprises.
- Objectif : pouvoir restaurer et compiler le projet sur un nouveau PC sans Internet, sans cache NuGet préalable et sans accès à GitHub/NuGet.org.
- Cette demande est volontairement mise à part et n'est pas réalisée dans la version 84.

## Dernière version générée

- Dernière archive générée : `AGRA-EASY-MOBILE-Version84.zip`.
- Base précédente : `AGRA-EASY-MOBILE-Version83.zip`.
- La version 84 corrige l'erreur de compilation `CS0103` liée à `ScanProductBarcodeAsync` dans `ProductCodeSelectionPage.cs`.

## Point de reprise Version84

- Base courante à reprendre : `AGRA-EASY-MOBILE-Version84.zip`.
- Scanner code-barres article intégré en V83, correction de la page de sélection article en V84.
- Autonomie complète des dépendances NuGet/GitHub encore à traiter séparément.

## Principales évolutions de la version 85

- Ajustement visuel des champs de référence article avec scanner dans les filtres expédition, retour et factures client.
- Les champs article conservent la hauteur de référence utilisée avant l'ajout du scanner ; le bouton scanner et le bouton recherche sont compactés dans la même zone.
- `ProductCodeSelectionPage` conserve son comportement scanner, avec un bouton visuellement réduit.
- Aucune modification fonctionnelle du scanner code-barres ni du scénario `FindProductCodeList(..., IsGenCode=true)`.

## Dernière version générée

- Dernière archive générée : `AGRA-EASY-MOBILE-Version85.zip`.
- Base précédente : `AGRA-EASY-MOBILE-Version84.zip`.

## Point de reprise Version85

- Base courante à reprendre : `AGRA-EASY-MOBILE-Version85.zip`.
- Scanner code-barres article intégré ; ajustement visuel des champs article avec scanner réalisé en V85.
- Autonomie complète des dépendances NuGet/GitHub encore à traiter séparément.

## Version 86 — Points de reprise

- Les champs article équipés du scanner doivent rester à la même hauteur que les autres champs de filtre.
- Les icônes scanner/recherche article sont compactes et empilées dans la zone droite du champ.
- Ne pas réagrandir les champs article lors de prochaines évolutions.
- Les zones code client invisibles pour les profils clients doivent rester positionnées en fin de grille pour ne pas créer de trou au milieu des filtres.
- La demande d'autonomie NuGet/GitHub complète reste à part : la dépendance scanner n'est pas encore packagée en mode `.nupkg` local offline complet.

## Point de reprise Version87

- Dernière archive générée : `AGRA-EASY-MOBILE-Version87.zip`.
- Base précédente : `AGRA-EASY-MOBILE-Version86.zip`.
- `MainPage` est désormais une page d'accueil condensée / menu guide avec bouton notifications et raccourcis métiers.
- Les champs `Référence article` avec scanner/recherche doivent conserver la hauteur normale du champ : texte à gauche, colonne actions à droite, scanner en haut, recherche en bas.
- Dans les vues de filtres, les zones client doivent rester placées en fin de grille, en bas, pour éviter les trous lorsque les profils clients masquent ces champs.
- `CustomerBillingFilterView` a été réorganisée pour supprimer les conflits de grille et placer les zones client en bas.
- La demande d'autonomie totale NuGet/GitHub reste mise à côté, non intégrée faute de `.nupkg` locaux complets.


## Demande active planifiée — accueil, logo et navigation

- Corriger l'intégration du logo AGRA/EASY sur la page d'accueil : harmoniser le fond ou retravailler le rendu pour éviter l'effet mal intégré.
- Appliquer la même correction au logo du menu latéral.
- Rendre le logo du menu latéral cliquable comme raccourci vers la page d'accueil.
- Revoir la place du raccourci `Frais de port` pour l'intégrer correctement dans la logique de menu facturation, sans perdre l'accès direct utile depuis l'accueil.
- Remplacer le titre `Accueil`, jugé trop simpliste, par un titre plus professionnel et représentatif de l'application mobile.
- Ne pas modifier les services web : aucun changement de documentation service web nécessaire pour cette demande.

## Dernière version générée — V88

- Page d'accueil retravaillée en menu guide condensé avec quatre raccourcis pleine largeur : Catalogue et commandes, Expéditions, Retours et garanties, Facturations et frais de port.
- Titre de la page d'accueil remplacé par `EASY Mobile`.
- Logo AGRA nettoyé avec fond transparent et utilisé dans l'accueil et le menu latéral.
- Logo du menu latéral cliquable pour revenir vers la page d'accueil.
- Image de démarrage remplacée par une image issue de la carte France/réseau DROP fournie, avec suppression visuelle du logo Sirius.
- Fond très léger de la carte DROP dans la zone des raccourcis de l'accueil.
- Pas de changement WebService dans cette version.

## Version 89 générée

- Page d'accueil ajustée : espace entre logo et titre, quatre encadrés d'identité `Groupement AGRA`, `Réseau DROP`, `Réseau PROXIMICA`, `Réseau PPOINT-REPAR`.
- Champs article avec scanner corrigés : colonne de droite partagée équitablement entre scanner et recherche, sans modification de la taille globale du champ.
- Aucun changement WebService ; documentation services web inchangée.

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
## Version 91 générée

- `AGRA-EASY-MOBILE.csproj` contient `Version=1.91`, `PackageVersion=1.91`, `ApplicationDisplayVersion=1.91`, `ApplicationVersion=91`.
- Correction Android `.arsc` conservée.
- Surveillance des alertes corrigée : sans date persistante de dernier lancement, toute alerte retournée est considérée comme nouvelle.
- La date persistante de dernier lancement des alertes n’est mise à jour qu’à l’ouverture réelle de la vue Alertes.
- Contrôle immédiat des alertes au lancement du Shell, puis toutes les 10 minutes.
- Animation des boutons Alertes sur l’accueil et le menu latéral en présence de nouvelles alertes.
- Correction `ShippingWarningFilterView` maintenue avec `FormatFilterDate(...)`.

## Reprise — Version 92

- Base livrée : V92.
- Les alertes doivent être contrôlées immédiatement après connexion réussie et à l’apparition de l’accueil, en plus du contrôle périodique toutes les 10 minutes.
- La surveillance automatique ne doit pas créer la date de dernier lancement de la vue Alertes.
- En absence de date persistante de dernier lancement, toute alerte récupérée par la surveillance est considérée comme nouvelle.
- `ShippingWarningFilterView.xaml.cs` utilise `FormatFilterDate(...)` pour l’affichage des dates.
- Liste alertes compacte : date + ShortDescription uniquement, sans client ni plateforme.
- Détail alerte : client discret dans le corps, `AccountCode - AccountName`, visible uniquement pour administrateur.

## Dernière version générée — V93

- Identifiant applicatif : `fr.groupeagra.easymobile`.
- Version : `0.93`.
- Code version Android : `93`.
- Correction `ShippingWarningFilterView.xaml.cs` : `FormatFilterDate(...)` utilisé aussi après sélection calendrier.
- Image de démarrage recadrée et affichée en plein écran dans `StartupConnectionView` sans déformation.

## Dernière version générée — V94

- Identifiant applicatif conservé : `fr.groupeagra.easymobile`.
- Version : `0.94`.
- Code version Android : `94`.
- Références générées du service web remplacées par les fichiers fournis : `ConnectedService.json` et `Reference.cs`.
- La classe générée `SorderBasketLine` ne contient plus `Price`, `Discount`, `NewPrice`, `NewDiscount`, `AdditionalDiscount`, `IsNet`, `GarageProductPrice` et `GarageProductDiscount`.
- Correction ciblée de `OrderBasketView.cs` : affichage du prix/remise panier à partir de `ProductPrice` et `ProductDiscount`.
- Documentation service web conservée sous forme versionnée : `Documentation_Service_Web_AGRA_EASY_MOBILE_Par_Scenarios_Version94.docx`.
- Compilation réelle non exécutée dans l’environnement de génération car `dotnet` n’est pas disponible.


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
