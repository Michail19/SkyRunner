# Реализация игровых механик SkyRunner

Этот документ кратко описывает, как в проекте реализованы основные игровые системы: генерация карты, исчезающие блоки, сбор предметов, появление врагов, выход с уровня и настройка сложности.

## Общая идея игры

SkyRunner — это арена-раннер на Unity, где игрок появляется на островной карте из отдельных плиток. Во время игры часть плиток постепенно исчезает, на карте появляются собираемые предметы, а враги пытаются вытолкнуть игрока за пределы платформы.

Основной игровой цикл:

1. Генерируется арена из плиток.
2. Игрок появляется в безопасной центральной зоне.
3. На карте появляются предметы.
4. Через некоторое время плитки начинают исчезать.
5. Периодически появляются враги.
6. После сбора всех предметов активируется выход.
7. Игрок побеждает, если добирается до выхода.
8. Игрок проигрывает, если падает с карты.

---

## Основные скрипты

| Скрипт                   | Назначение                                                  |
| ------------------------ | ----------------------------------------------------------- |
| `ArenaGenerator.cs`      | Генерирует карту из отдельных плиток                        |
| `ArenaTile.cs`           | Хранит состояние одной плитки и отвечает за её исчезновение |
| `TileCollapseManager.cs` | Выбирает плитки, которые должны исчезнуть                   |
| `ObjectiveManager.cs`    | Создаёт собираемые предметы и активирует выход              |
| `CollectibleItem.cs`     | Отвечает за подбор предмета игроком                         |
| `ExitZone.cs`            | Проверяет вход игрока в зону выхода                         |
| `BotSpawner.cs`          | Создаёт врагов волнами                                      |
| `SimpleBotPusher.cs`     | Управляет поведением врага                                  |
| `GameSettings.cs`        | Хранит настройки сложности                                  |
| `MenuController.cs`      | Применяет настройки из меню                                 |
| `GameManager.cs`         | Управляет победой, поражением и перезапуском                |

---

## Генерация карты

Карта создаётся скриптом `ArenaGenerator`.

В сцене задаётся базовый размер карты:

```csharp
public int gridSize = 18;
public float tileSize = 2f;
```

Генератор проходит по сетке `gridSize x gridSize`, создаёт плитки в форме острова. Для этого считается расстояние от текущей клетки до центра карты. Если клетка слишком далеко от центра, плитка не создаётся.

Логика выглядит так:

```csharp
float distanceFromCenter = Mathf.Sqrt(
    Mathf.Pow(x - centerOffset, 2) +
    Mathf.Pow(z - centerOffset, 2)
);

if (distanceFromCenter > radius)
{
    continue;
}
```

Так получается круглая (островная) форма карты.

---

## Типы плиток

В проекте используются разные типы плиток:

| Тип плитки     | Назначение                              |
|----------------| --------------------------------------- |
| `Grass`        | Обычная плитка                          |
| `Water` / Low  | Низкая плитка, замедляет игрока         |
| `Stone` / High | Каменная плитка, может немного ускорять |

Тип плитки выбирается на основе Perlin Noise. Благодаря этому карта получается с небольшим разнообразием высот и поверхностей.

Пример логики:

```csharp
private int GetHeightLevel(int x, int z, bool isProtected)
{
    // Центральные 4 клетки всегда плоские и травяные.
    if (isProtected)
    {
        return 0;
    }

    if (!useHeightVariation)
    {
        return 0;
    }

    float noise = Mathf.PerlinNoise(
        (x + 1000) * noiseScale,
        (z + 1000) * noiseScale
    );

    if (noise < lowThreshold)
    {
        return -1;
    }

    if (noise > highThreshold)
    {
        return 1;
    }

    return 0;
}
```

---

## Безопасная центральная зона

В центре карты создаётся защищённая зона. Она нужна, чтобы игрок не появился сразу на исчезающей плитке или рядом с опасностью.

За это отвечает параметр:

```csharp
public int protectedCenterSize = 2;
```

Плитки в центре получают флаг:

```csharp
isProtected = true;
```

Такие плитки не выбираются системой исчезновения и не используются для размещения предметов или опасных объектов.

---

## Автомасштабирование карты от сложности

Размер карты масштабируется в зависимости от выбранной сложности.

В `ArenaGenerator` используются параметры:

```csharp
public bool scaleGridWithDifficulty = true;
public float easyGridScale = 0.5f;
public float normalGridScale = 1f;
public float hardGridScale = 1.5f;
public int minGridSize = 12;
public int maxGridSize = 96;
```

Логика:

| Сложность |    Масштаб карты |
| --------- | ---------------: |
| Easy      | `gridSize * 0.5` |
| Normal    |   `gridSize * 1` |
| Hard      | `gridSize * 1.5` |

Например, если базовый `gridSize = 54`, то получится:

| Сложность | Итоговый размер |
| --------- | --------------: |
| Easy      |              28 |
| Normal    |              54 |
| Hard      |              82 |

Размер дополнительно ограничивается через `minGridSize` и `maxGridSize`, чтобы карта не стала слишком маленькой или слишком большой.

---

## Исчезающие блоки

Исчезающие блоки реализованы через связку двух скриптов:

* `TileCollapseManager.cs`
* `ArenaTile.cs`

`TileCollapseManager` не удаляет плитки сам. Он только выбирает подходящие плитки и вызывает у них метод исчезновения.

Плитка может быть выбрана для исчезновения, если она:

* существует;
* ещё не уничтожена;
* не является защищённой центральной плиткой;
* не содержит предмет;
* находится не слишком близко к игроку.

Логика выбора:

```csharp
foreach (ArenaTile tile in arenaGenerator.tiles)
{
    if (tile == null)
    {
        continue;
    }

    if (tile.isDestroyed || tile.isProtected || tile.hasObjectiveItem)
    {
        continue;
    }

    float distanceToPlayer = Vector3.Distance(tile.transform.position, player.position);

    if (distanceToPlayer < minDistanceFromPlayer)
    {
        continue;
    }

    candidates.Add(tile);
}
```

После этого случайные плитки из списка кандидатов начинают исчезать:

```csharp
selectedTile.Collapse(warningTime);
```

---

## Предупреждение перед исчезновением

Перед исчезновением плитка не пропадает сразу. Сначала она переходит в состояние предупреждения.

Обычно это делается так:

1. Плитка меняет материал на предупреждающий.
2. Игра ждёт `warningTime`.
3. Плитка отключает collider.
4. Плитка исчезает визуально.
5. Плитка помечается как уничтоженная.

Это нужно, чтобы игрок успел заметить опасность и уйти с плитки.

Параметры системы:

```csharp
public float startDelay = 5f;
public float warningTime = 1.5f;
public float pauseAfterCollapse = 1.5f;
public int tilesPerWave = 1;
```

---

## Волны исчезновения плиток

Плитки исчезают не все сразу, а волнами. Логика работы:

```csharp
yield return new WaitForSeconds(startDelay);

while (isRunning)
{
    CollapseRandomTiles();

    yield return new WaitForSeconds(warningTime);
    yield return new WaitForSeconds(pauseAfterCollapse);

    UpdateDifficulty();
}
```

Сначала игра ждёт `startDelay`, чтобы игрок успел осмотреться. Потом каждая волна выбирает несколько плиток и запускает их исчезновение.

---

## Усложнение исчезающих плиток

Во время игры система может постепенно повышать сложность:

```csharp
public bool increaseDifficulty = true;
public float difficultyStepTime = 15f;
public int maxTilesPerWave = 4;
public float minPauseAfterCollapse = 0.5f;
```

Через заданный интервал:

* увеличивается количество исчезающих плиток за волну;
* уменьшается пауза между волнами.

Так арена становится опаснее по мере прохождения.

---

## Сбор предметов

Предметы создаются скриптом `ObjectiveManager`.

Он выбирает случайные плитки, подходящие для размещения предметов. Плитка подходит, если она:

* не уничтожена;
* не защищена;
* находится достаточно далеко от центра;
* ещё не занята другим предметом.

Параметры:

```csharp
public int itemsToSpawn = 3;
public float itemHeightOffset = 1f;
public float minDistanceFromCenter = 6f;
```

Количество предметов берётся из `GameSettings` и зависит от сложности.

Пример:

| Сложность | Количество предметов |
| --------- | -------------------: |
| Easy      |                    2 |
| Normal    |              3 или 4 |
| Hard      |         5 или больше |

После создания предмета плитка помечается как занятая, чтобы система исчезающих блоков не уничтожила плитку с важным предметом.

---

## Подбор предмета

Когда игрок касается предмета, вызывается метод сбора.

Общая логика:

1. Увеличить счётчик собранных предметов.
2. Удалить предмет со сцены.
3. Очистить связь предмета с плиткой.
4. Обновить UI.
5. Если собраны все предметы — открыть выход.

Логика:

```csharp
public void CollectItem(CollectibleItem item)
{
    if (item == null)
    {
        return;
    }

    collectedItems++;

    spawnedItems.Remove(item);
    item.ClearOwnerTile();
    Destroy(item.gameObject);

    if (collectedItems >= itemsToSpawn)
    {
        ActivateExit();
    }

    UpdateUI();
}
```

---

## Активация выхода

В начале уровня выход отключён:

```csharp
exitZoneObject.SetActive(false);
```

После сбора всех предметов вызывается:

```csharp
ActivateExit();
```

Метод включает объект выхода:

```csharp
exitZoneObject.SetActive(true);
```

После этого игрок может войти в зону выхода и завершить уровень победой.

---

## Враги

Враги создаются через `BotSpawner`.

Спавн работает волнами. Через определённый интервал спавнер проверяет, сколько врагов уже живо, и создаёт новых, если лимит не превышен.

Основные параметры:

```csharp
public float startDelay = 3f;
public float spawnInterval = 8f;
public int botsPerWave = 1;
public int maxAliveBots = 5;
```

Спавнер не создаёт врагов рядом с игроком. Для этого используется параметр:

```csharp
public float minDistanceFromPlayer = 8f;
```

Также враги появляются ближе к краю карты, чтобы не возникать прямо в центре.

---

## Выбор места появления врага

Для спавна врага выбирается случайная плитка, которая:

* существует;
* не уничтожена;
* не защищена;
* находится ближе к краю карты;
* находится достаточно далеко от игрока.

Логика:

```csharp
if (distanceFromCenter < maxDistanceFromCenter * edgeSpawnPercent)
{
    continue;
}

float distanceToPlayer = Vector3.Distance(
    tile.transform.position,
    player.position
);

if (distanceToPlayer < minDistanceFromPlayer)
{
    continue;
}

candidates.Add(tile);
```

После выбора плитки враг создаётся немного выше поверхности:

```csharp
Vector3 spawnPosition = spawnTile.transform.position + Vector3.up * spawnHeightOffset;
```

---

## Поведение врагов

Враг управляется скриптом `SimpleBotPusher`.

Его основная задача:

1. Найти игрока.
2. Двигаться в сторону игрока.
3. При сближении атаковать.
4. Толкнуть игрока.
5. Не падать в уже разрушенные зоны, если включена такая проверка.

Скорость врага может изменяться через сложность:

```csharp
bot.moveSpeed *= GameSettings.botMoveSpeedMultiplier;
```

---

## Настройки сложности

Сложность хранится в `GameSettings`.

Основные уровни:

```csharp
public enum GameDifficulty
{
    Easy,
    Normal,
    Hard
}
```

Для каждой сложности задаются:

* количество предметов;
* задержка перед началом разрушения плиток;
* время предупреждения;
* количество исчезающих плиток;
* интервал появления врагов;
* максимальное количество живых врагов;
* множитель скорости врагов.

Пример:

```csharp
case GameDifficulty.Easy:
    itemsToCollect = 2;

    collapseStartDelay = 8f;
    warningTime = 2f;
    pauseAfterCollapse = 2f;
    tilesPerWave = 1;

    botSpawnInterval = 10f;
    maxAliveBots = 2;
    botMoveSpeedMultiplier = 0.85f;
    break;

case GameDifficulty.Normal:
    itemsToCollect = 3;

    collapseStartDelay = 5f;
    warningTime = 1.5f;
    pauseAfterCollapse = 1.5f;
    tilesPerWave = 1;

    botSpawnInterval = 8f;
    maxAliveBots = 4;
    botMoveSpeedMultiplier = 1f;
    break;

case GameDifficulty.Hard:
    itemsToCollect = 5;

    collapseStartDelay = 3f;
    warningTime = 1.1f;
    pauseAfterCollapse = 0.8f;
    tilesPerWave = 2;

    botSpawnInterval = 5f;
    maxAliveBots = 7;
    botMoveSpeedMultiplier = 1.15f;
    break;
```

---

## Применение сложности из меню

Меню использует `MenuController`.

При выборе сложности вызывается:

```csharp
SetDifficulty(int index)
```

Дальше сложность сохраняется в `PlayerPrefs`:

```csharp
PlayerPrefs.SetInt(DifficultyKey, (int)difficulty);
```

Перед запуском уровня меню применяет текущие настройки и загружает сцену:

```csharp
ApplyCurrentMenuSettings();
SceneManager.LoadScene("Level");
```

На уровне игровые менеджеры вызывают:

```csharp
GameSettings.Load();
```

После этого они применяют актуальные значения сложности.

---

## Какие параметры менять для настройки баланса

### Размер карты

Скрипт: `ArenaGenerator`

```csharp
gridSize
easyGridScale
normalGridScale
hardGridScale
minGridSize
maxGridSize
```

Рекомендуемый вариант:

| Параметр          | Значение |
| ----------------- | -------: |
| `gridSize`        |       36 |
| `easyGridScale`   |      0.5 |
| `normalGridScale` |        1 |
| `hardGridScale`   |      1.5 |

Тогда получится:

| Сложность | Размер карты |
| --------- | -----------: |
| Easy      |           18 |
| Normal    |           36 |
| Hard      |           54 |

---

### Исчезающие плитки

Скрипт: `TileCollapseManager`

```csharp
startDelay
warningTime
pauseAfterCollapse
tilesPerWave
maxTilesPerWave
minPauseAfterCollapse
difficultyStepTime
```

Чем меньше `warningTime` и `pauseAfterCollapse`, тем сложнее игра.

---

### Предметы

Скрипт: `ObjectiveManager`

```csharp
itemsToSpawn
minItems
maxItems
autoScaleWithArena
```

Если нужно, чтобы сложность напрямую задавала количество предметов, лучше отключить:

```csharp
autoScaleWithArena = false;
```

Если оставить автоскейлинг, количество предметов будет зависеть ещё и от размера карты.

---

### Враги

Скрипт: `BotSpawner`

```csharp
spawnInterval
botsPerWave
maxAliveBots
maxBotsPerWave
maxAliveBotsLimit
minSpawnInterval
```

Чтобы сложная сложность ощущалась сильнее, нужно:

* уменьшить `spawnInterval`;
* увеличить `maxAliveBots`;
* увеличить `botsPerWave`;
* увеличить `maxBotsPerWave`;
* увеличить `botMoveSpeedMultiplier`.

---

## Возможные проблемы

### Normal и Hard выглядят одинаково

Причина может быть в том, что параметры упираются в максимальные лимиты.

Например:

```csharp
maxItems = 8;
maxMaxAliveBots = 15;
```

Если карта большая, автоскейлинг может привести к тому, что Normal и Hard оба достигнут одного и того же максимума.

Решение:

* увеличить лимиты;
* отключить `autoScaleWithArena`;
* уменьшить базовый `gridSize`;
* сильнее развести параметры в `GameSettings`.

---

### Слишком низкий FPS

Возможные причины:

* слишком большой `gridSize`;
* слишком много плиток;
* слишком много врагов;
* частый спавн;
* много исчезающих плиток;
* включены тяжёлые тени;
* используются сложные материалы или эффекты.

Решение:

* уменьшить `gridSize`;
* уменьшить `hardGridScale`;
* ограничить `maxGridSize`;
* снизить `maxAliveBots`;
* увеличить `spawnInterval`;
* упростить настройки качества.
