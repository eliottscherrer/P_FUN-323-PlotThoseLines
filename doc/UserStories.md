# 📂 Users Stories – P_FUN-323-PlotThoseLines

## [#1 – \[EPIC\] Ingestion des données et API](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/1)

### [#2 – Lister les marchés crypto](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/2)

📝 **En tant que** développeur
**Je souhaite** appeler l’API TokenInsight pour récupérer la liste des cryptos disponibles et normaliser la réponse en modèle `MarketInfo`.
**Pour** disposer d’une source de données que l’UI peut afficher.

📌 **Tests d’acceptance**

- Le backend appelle `api/v1/coins/list`.
- Transformation en `MarketInfo` (id, symbole, nom, prix, volume 24h…).
- Gestion d’erreurs avec feedback clair.
- Exécution < 2s, tri par symbole alphabétique.

---

### [#3 – Récupération des données historiques](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/3)

📝 **En tant que** utilisateur
**Je souhaite** récupérer des séries temporelles historiques depuis TokenInsight.
**Pour** alimenter les graphiques avec de vraies données.

📌 **Tests d’acceptance**

- Appel `/api/v1/history/coins/{id}`, transformation en `TimeSeriesPoint`.
- Gestion des erreurs (logs + UI informée).
- Séries récupérées < 2s, triées par timestamp.

---

## [#9 – \[EPIC\] Importation des données](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/9)

### [#8 – Importation de données JSON local](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/8)

📝 **En tant que** utilisateur
**Je souhaite** importer un asset local depuis un fichier JSON.
**Afin de** visualiser mes propres données dans l’application.

📌 **Tests d’acceptance**

- Fichier JSON valide → asset importé avec nom, symbole et logo.
- L’asset apparaît dans ma liste locale.

---

### [#13 – Importation des données API](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/13)

📝 **En tant que** utilisateur
**Je souhaite** importer un asset via API en le recherchant par nom.
**Afin de** accéder à des données crypto à jour automatiquement.

📌 **Tests d’acceptance**

- Je recherche un asset (ex. Ethereum) puis clique _Add new asset_.
- L’asset apparaît dans ma liste API avec ses données.

---

### [#14 – Expérience utilisateur lors de l'importation d'assets](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/14)

📝 **En tant que** utilisateur
**Je souhaite** être guidé par l’interface lors de l’importation (états vides, erreurs, confirmation).
**Afin de** éviter les erreurs et savoir si l’ajout a réussi.

📌 **Tests d’acceptance**

- Formulaire incomplet, fichier invalide ou doublon → message clair affiché.
- Succès ou échec mis à jour dans la liste.

---

### [#15 – Clarté de la page d'Assets](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/15)

📝 **En tant que** utilisateur
**Je souhaite** que l’UI sépare clairement assets locaux et API.
**Afin de** gérer mes imports sans confusion.

📌 **Tests d’acceptance**

- Présentation claire : distinction visuelle avec noms, symboles, icônes.

---

## [#4 – \[EPIC\] Visualisation & Interaction](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/4)

### [#5 – Affichage multi-séries temporelles](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/5)

📝 **En tant que** analyste
**Je souhaite** afficher plusieurs séries temporelles superposées.
**Pour** comparer prix, volume, indicateurs facilement.

📌 **Tests d’acceptance**

- Affichage simultané d’au moins 2 séries (couleurs distinctes, axe Y flexible).
- Tooltips précis.
- Séries alignées sur le temps même avec données manquantes.

---

### [#6 – Sélectionner des intervalles](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/6)

📝 **En tant que** utilisateur
**Je souhaite** sélectionner des intervalles prédéfinis (1d, 1w, 1m, 1y, All).
**Pour** explorer les données à différentes échelles.

📌 **Tests d’acceptance**

- Boutons visibles pour chaque intervalle.
- Graphiques mis à jour automatiquement.
- Données alignées correctement.
- Intervalle sélectionné mis en évidence.

---

### [#7 – Superposer des périodes différentes](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/7)

📝 **En tant que** analyste
**Je souhaite** comparer des séries couvrant différentes périodes (ex. 2020 vs 2021).
**Pour** analyser saisonnalités et comportements périodiques.

📌 **Tests d’acceptance**

- Séries superposées distinctement (style, couleur, légende).
- Alignement temporel normalisé.
- Tooltips indiquant date et valeur.

---

## [#10 – \[Task\] Maquettes Figma](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/10)

📝 **En tant que** développeur
**Je souhaite** concevoir et utiliser des maquettes Figma de l’application.
**Pour** avoir une vision claire et validée avant de coder.

📌 **Tests d’acceptance**

- Écrans _Importation de données_, _Graphiques_ et _Paramètres_ créés dans Figma.
- Navigation visible et cohérente.
- Composants identifiés.
- Alignement avec les User Stories.

---

## [#16 – Page Settings](https://github.com/eliottscherrer/P_FUN-323-PlotThoseLines/issues/16)

📝 **En tant que** utilisateur
**Je souhaite** renseigner et gérer ma clé API dans _Settings_.
**Afin de** connecter l’application à mes sources externes.

📌 **Tests d’acceptance**

- Champ API visible dans _Settings_.
- Clé API sauvegardée et utilisée pour récupérer mes données.
