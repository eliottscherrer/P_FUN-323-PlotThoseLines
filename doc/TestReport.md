# 📋 Rapport de test : PlotThoseLines

- **Date du test** : 02.11.2025  
- **Nom du testeur** : Eliott Scherrer  
- **Référentiel de tests** : xUnit (.NET)

# 🧪 Scénarios détaillés (Arrange / Act / Assert)

## 1. Détermination de l’intervalle minimal (heure)

`GetMinimumDataInterval_WithHourlyIntervals_ReturnsHour`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| Un historique de 3 points journaliers, espacés d'une heure: 2024-01-01 12:00, 13:00, 14:00. Les prix sont légèrement croissants. | On demande au service de calculer l'intervalle minimal (le plus petit pas de temps) présent dans ces données. | Le service renvoie "hour". |

**Résultat** :
- [x] OK
- [ ] KO

## 2. Détermination de l’intervalle minimal (journalier)

`GetMinimumDataInterval_WithDailyIntervals_ReturnsDay`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| Un historique de 3 points journaliers: 2024-01-01, 2024-01-02, 2024-01-03. | On demande au service de calculer l'intervalle minimal sur ces données. | Le service renvoie "day". |

**Résultat** :
- [x] OK
- [ ] KO

## 3. Modèle LocalAsset : propriétés de base

`LocalAsset_WithValidId_HasPropertiesSet`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| On crée une crypto fictive avec Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin" (sans appel API). | On instancie `LocalAsset` avec ces valeurs. | Les propriétés `Id`, `Symbol`, `Name` correspondent exactement aux valeurs fournies. |

**Résultat** :
- [x] OK
- [ ] KO

## 4. Modèle LocalAsset : accès à l’historique

`LocalAsset_WithHistoryData_PriceIsAccessible`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| Un `LocalAsset` dont `HistoryData` contient un unique point: date = 2024-01-01, price = 42000.0. | On lit le premier (et seul) élément de `HistoryData`. | La liste existe, contient 1 élément, et la paire (date, prix) correspond exactement. |

**Résultat** :
- [x] OK
- [ ] KO

## 5. SettingsService : récupération de la clé API

`GetApiKey_ReturnsNonNullValue`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| Un `SettingsService` initialisé. | On lit la clé API persistée via le service. | La valeur renvoyée n'est pas `null`. |

**Résultat** :
- [x] OK
- [ ] KO

## 6. SettingsService : persistance de la clé API

`SetApiKeyAsync_SavesAndRetrievesKey`

| Arrange / Given | Act / When | Assert / Then |
| --- | --- | --- |
| Un `SettingsService` et une valeur de test: "test-api-key-123". | On enregistre cette clé via le service puis on la relit depuis le même service. | La valeur lue est exactement "test-api-key-123". |

**Résultat** :
- [x] OK
- [ ] KO

# 📊 Résultats globaux & recommandations

- **Scénarios 1 à 6 :** ✅ OK 
- **Recommandation (go / nogo)** : GO
- **Date et signature du testeur** : 02.11.2025 - Eliott Scherrer

# 🧑‍💻 Code des tests

Liens vers les fichiers de tests unitaires :  

- [LocalAssetService](../tests/PlotThoseLines.Tests/Services/LocalAssetServiceTests.cs): trouver l’intervalle minimal à afficher dans le graphique par rapport aux données disponibles  
- [LocalAsset (validation)](../tests/PlotThoseLines.Tests/Services/LocalAssetValidationTests.cs): propriétés et historique  
- [SettingsService](../tests/PlotThoseLines.Tests/Services/SettingsServiceTests.cs): lecture et persistance disque de la clé API  
