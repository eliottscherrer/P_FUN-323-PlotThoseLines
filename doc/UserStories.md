# User Stories

### US1 : En tant qu’utilisateur, je veux importer des données de cryptomonnaies depuis une API pour les afficher dans PTL.

- Critères d’acceptation :
  - PTL se connecte à une API publique (TokenInsight).
  - L’utilisateur peut sélectionner une ou plusieurs cryptomonnaies (BTC, ETH, etc.).
  - Les données sont importées au format JSON.

### US2 : En tant qu’utilisateur, je veux afficher un graphique comparant plusieurs cryptomonnaies sur une période donnée.

- Critères d’acceptation :
  - L’utilisateur peut choisir une plage de dates (ex: 1 mois, 1 an, personnalisé).
  - Le graphique affiche les prix des cryptos sélectionnées sur un axe temporel commun.
  - On peut zoomer/dézoomer et survoler les points pour voir les valeurs exactes.

### US3 : En tant qu’utilisateur, je veux superposer des intervalles de temps pour une même crypto (ex: BTC en 2020 et 2021).

- Critères d’acceptation :
  - PTL permet de charger plusieurs jeux de données pour une même crypto (deux appels API).
  - Les données sont affichées en continu sur le graphique, avec une distinction visuelle (couleur, style de ligne).

### US4 : En tant qu’utilisateur, je veux afficher des fonctions mathématiques en plus des séries temporelles.

- Critères d’acceptation :
  - Onglet ou mode dédié pour entrer une fonction (ex: `sin(x)`, `x^2`).
  - La fonction est tracée sur le graphique, avec possibilité de superposition avec les données de crypto.
  - Utilisation de Roslyn pour évaluer dynamiquement les expressions C#.

## Planification

1. Semaine 1-2 : Recherche d’API, setup du projet GitHub, création des maquettes.
2. Semaine 3-4 : Développement de l’import des données (US1) et affichage basique (US2).
3. Semaine 5-6 : Ajout des fonctionnalités de personnalisation (US4) et superposition (US3).
4. Semaine 7-8 : Intégration des fonctions mathématiques (US5) et export (US6).
5. Semaine 9 : Tests, documentation, et préparation de la release.
