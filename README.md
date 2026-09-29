# Steam Release Analytics API

[![CI](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain) [![Tests](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FSourceSurfer%2FSteamReleaseAnalytics%2Fbadges%2Ftests.json)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain)

Backend-сервис на ASP.NET Core для хранения и анализа данных о релизах игр в Steam: календарь релизов по месяцам, статистика и динамика популярности жанров. Данные об играх вносятся через REST API; автоматический сбор из Steam — в планах (см. «Будущие улучшения»).

## 🎯 Основные возможности

- **Календарь релизов** - получение игр по месяцам с группировкой по дням
- **Аналитика жанров** - топ-5 популярных жанров с статистикой
- **Анализ динамики** - отслеживание изменений популярности жанров за 3 месяца
- **JWT аутентификация** - POST и DELETE требуют токен (демо-логин, см. «Аутентификация»)
- **REST API** - стандартный API для интеграции с фронтенд-приложениями
- **Swagger документация** - интерактивная документация API

## 🛠️ Технологический стек

- **Backend**: C# и ASP.NET Core 10 (.NET 10 LTS)
- **ORM**: Entity Framework Core 10 + Npgsql, миграции в репозитории
- **Database**: PostgreSQL 16
- **Аутентификация**: JWT токены
- **Контейнеризация**: Docker и Docker Compose
- **API документация**: Swagger / OpenAPI (Swashbuckle)
- **Тесты**: xUnit, NSubstitute, FluentAssertions, EF Core InMemory
- **Архитектура**: монолит с разделением на проекты Api / Services / Infrastructure / Core

## 📋 Структура проекта

```
SteamReleaseAnalytics/
├── SteamReleaseAnalytics.Api/           # REST API и контроллеры
│   ├── Controllers/
│   │   ├── GamesController.cs           # Эндпоинты для работы с играми
│   │   ├── AnalyticsController.cs       # Эндпоинты аналитики
│   │   └── AuthController.cs            # Аутентификация
│   ├── Program.cs                       # Конфигурация приложения
│   ├── SteamReleaseAnalytics.Api.http   # Готовые запросы к API (VS / Rider / VS Code REST Client)
│   └── appsettings.json                 # Настройки
├── SteamReleaseAnalytics.Core/          # Модели и DTO
│   ├── Models/                          # Entity модели
│   │   ├── Game.cs
│   │   ├── Tag.cs
│   │   ├── GameTag.cs
│   │   └── GameSnapshot.cs
│   └── Dtos/                            # Data Transfer Objects
│       ├── GameDto.cs
│       ├── CreateGameDto.cs
│       ├── GameCalendarDto.cs
│       ├── GenreStatsDto.cs
│       └── GenreDynamicsDto.cs
├── SteamReleaseAnalytics.Infrastructure/# Работа с БД и репозитории
│   ├── Data/
│   │   └── SteamDbContext.cs            # Entity Framework контекст
│   ├── Migrations/                      # Миграции БД (EF Core)
│   └── Repositories/                    # Репозитории
│       ├── IGameRepository.cs
│       ├── GameRepository.cs
│       ├── ITagRepository.cs
│       ├── TagRepository.cs
│       ├── IGameSnapshotRepository.cs
│       └── GameSnapshotRepository.cs
├── SteamReleaseAnalytics.Services/      # Бизнес-сервисы
│   ├── Services/
│   │   ├── IAnalyticsService.cs
│   │   └── AnalyticsService.cs
│   └── Security/
│       ├── IJwtTokenGenerator.cs
│       └── JwtTokenGenerator.cs
├── tests/SteamReleaseAnalytics.Tests/   # Юнит-тесты (xUnit, NSubstitute, FluentAssertions)
├── .github/workflows/ci.yml             # CI: сборка, тесты, значок числа тестов
├── dotnet-tools.json                    # Локальный инструмент dotnet-ef
├── docker-compose.yml                   # Docker Compose конфигурация
├── Dockerfile                           # Docker образ для API
└── README.md                            # Документация
```

## 🚀 Быстрый старт

### Требования

- Docker Desktop (или Docker + Docker Compose)
- .NET 10 SDK (для локальной разработки)

### Запуск через Docker Compose

1. **Перейдите в папку проекта**:
```bash
cd SteamReleaseAnalytics
```

2. **Запустите контейнеры**:
```bash
docker compose up --build
```

Это автоматически:
- Запустит PostgreSQL базу данных
- Запустит pgAdmin для управления БД
- Соберёт и запустит API приложение
- Применит миграции и создаст все необходимые таблицы

3. **Откройте приложение**:
- **API и Swagger**: http://localhost:8080/swagger/index.html
- **pgAdmin**: http://localhost:5050 (admin@example.com / admin)
- **PostgreSQL**: localhost:5432 (postgres / postgres)

### Запуск локально (API без Docker)

1. **Поднимите PostgreSQL** — из Docker Compose или свой PostgreSQL 16 с настройками из `appsettings.json`:
```bash
docker compose up -d postgres
```

2. **Запустите API**:
```bash
dotnet run --project SteamReleaseAnalytics.Api
```

Миграции применяются при старте автоматически (переменная `Database__ApplyMigrationsOnStartup` задана в `Properties/launchSettings.json`).

API будет доступна на: http://localhost:5268/swagger/index.html
(HTTPS: `dotnet run --project SteamReleaseAnalytics.Api --launch-profile https` → https://localhost:7205/swagger/index.html)

Применить миграции вручную, без запуска API:
```bash
dotnet tool restore
dotnet ef database update --project SteamReleaseAnalytics.Infrastructure --startup-project SteamReleaseAnalytics.Api
```

## 📚 API Эндпоинты

### Аутентификация

**POST** `/api/v1/auth/login`
- Получить JWT токен для аутентификации
- Body: `{"username": "admin"}`
- Response: `{"token": "eyJhbGc..."}`

### Игры

**GET** `/api/v1/games`
- Получить все игры
- Response: массив GameDto

**GET** `/api/v1/games/{id}`
- Получить игру по ID
- Response: GameDto

**POST** `/api/v1/games` (требует авторизацию)
- Создать новую игру
- Body: CreateGameDto
- Response: GameDto (код 201)

**DELETE** `/api/v1/games/{id}` (требует авторизацию)
- Удалить игру
- Response: код 204

**GET** `/api/v1/games/calendar?month=2025-11`
- Получить календарь релизов на месяц
- Response: GameCalendarDto

**GET** `/api/v1/games/by-tag/{tagName}`
- Получить игры по тегу/жанру
- Response: массив GameDto

### Аналитика

**GET** `/api/v1/analytics/top-genres?month=11&year=2025`
- Получить топ-5 жанров за месяц
- Response: массив GenreStatsDto

**GET** `/api/v1/analytics/genre-dynamics`
- Получить динамику изменений топ-5 жанров за последние 3 месяца
- Response: массив GenreDynamicsDto

## 🔐 Аутентификация

API использует JWT токены для защиты изменяющих операций (POST, DELETE).

1. Получите токен через `/api/v1/auth/login`
2. Добавьте токен в заголовок Authorization:
```
Authorization: Bearer <ваш_токен>
```

Токен действителен 60 минут (настраивается в `appsettings.json`).

> **Демо-режим.** `/api/v1/auth/login` не проверяет пароль: токен с ролью `Admin` выдаётся для любого имени пользователя. Это сделано для удобства проверки API; для реального использования нужна проверка учётных данных.

## 🗄️ База данных

Схема создаётся миграциями EF Core из `SteamReleaseAnalytics.Infrastructure/Migrations`.

### Таблицы

- **Games** - основная таблица с информацией об играх
- **Tags** - теги/жанры игр
- **GameTags** - связь между играми и тегами (M:N)
- **GameSnapshots** - исторические данные о числе подписчиков (зарезервировано под сбор данных из Steam, API пока не использует)

### Схема базы данных

```sql
-- Games таблица содержит:
- SteamAppId (Primary Key)
- Title
- Description
- ReleaseDate
- ImageUrl
- StoreUrl
- Followers
- Platforms (Windows, Mac, Linux)
- CreatedAt
- UpdatedAt

-- Tags таблица содержит:
- Id (Primary Key)
- Name (Unique)

-- GameTags таблица содержит:
- Id (Primary Key)
- GameSteamAppId (FK to Games)
- TagId (FK to Tags)

-- GameSnapshots таблица содержит:
- Id (Primary Key)
- GameSteamAppId (FK to Games)
- FollowersCount
- SnapshotDate
```

## ✅ Как запустить тесты

```bash
dotnet test
```

Тесты не требуют ни PostgreSQL, ни сети: репозитории подменяются через NSubstitute, а сами репозитории проверяются на EF Core InMemory. Покрыты расчёт статистики жанров (`AnalyticsService`), выдача и проверка JWT (`JwtTokenGenerator`), выборка релизов по месяцу и тегу, календарь релизов и валидация запросов в контроллерах.

## 🧪 Тестирование API

### Через Swagger UI

1. Откройте http://localhost:8080/swagger/index.html
2. Используйте интерактивный интерфейс для тестирования эндпоинтов

### Через .http файл

`SteamReleaseAnalytics.Api/SteamReleaseAnalytics.Api.http` содержит готовые запросы ко всем эндпоинтам; токен из запроса логина подставляется автоматически. Адрес по умолчанию — локальный запуск (`http://localhost:5268`); для Docker замените `@host` на `http://localhost:8080`.

### Через Postman

1. **Получите токен**:
   - POST `http://localhost:8080/api/v1/auth/login`
   - Body: `{"username": "admin"}`

2. **Используйте токен**:
   - Добавьте заголовок: `Authorization: Bearer <token>`
   - Выполняйте POST/DELETE запросы

### Примеры запросов

**Создать игру**:
```bash
curl -X POST http://localhost:8080/api/v1/games \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "steamAppId": 1091500,
    "title": "Cyberpunk 2077",
    "description": "An open-world action RPG",
    "releaseDate": "2025-11-15T00:00:00Z",
    "imageUrl": "https://example.com/image.jpg",
    "storeUrl": "https://store.steampowered.com/app/1091500",
    "followers": 150000,
    "platforms": "Windows,Mac",
    "tags": ["Action", "RPG", "Sci-Fi"]
  }'
```

**Получить календарь на ноябрь**:
```bash
curl http://localhost:8080/api/v1/games/calendar?month=2025-11
```

**Получить топ жанров**:
```bash
curl "http://localhost:8080/api/v1/analytics/top-genres?month=11&year=2025"
```

## 📊 Архитектура

### Многоуровневая архитектура

1. **API Layer** (Controllers) - обработка HTTP запросов
2. **Service Layer** - бизнес-логика (Analytics, Security)
3. **Repository Layer** - работа с БД через Entity Framework
4. **Data Access Layer** - Entity Framework DbContext
5. **Database Layer** - PostgreSQL

### Паттерны

- **Repository Pattern** - для работы с БД
- **Dependency Injection** - через встроенный DI контейнер ASP.NET Core
- **DTO Pattern** - для передачи данных между слоями
- **JWT** - для аутентификации

## 🔧 Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=steam_releases;Username=postgres;Password=postgres;"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-change-this-in-production-at-least-32-characters-long!!!",
    "ExpirationMinutes": 60,
    "Issuer": "SteamReleaseAnalytics",
    "Audience": "SteamReleaseAnalyticsUsers"
  }
}
```

Значения в `appsettings.json` — локальные значения по умолчанию для разработки, не секреты.

| Параметр | Назначение |
|---|---|
| `ConnectionStrings:DefaultConnection` | Строка подключения к PostgreSQL |
| `JwtSettings:*` | Ключ подписи, срок жизни, издатель и аудитория токенов |
| `Database:ApplyMigrationsOnStartup` | `true` — применять миграции при старте (по умолчанию выключено; включено в `launchSettings.json` и `docker-compose.yml`) |

## 🚨 Важные замечания

- **Среда**: Production в Docker (без HTTPS для упрощения)
- **Безопасность**: В Production используйте настоящие сертификаты HTTPS
- **Секрет JWT**: Измените `JwtSettings.Secret` на длинный, случайный ключ (например, через переменную окружения `JwtSettings__Secret`)
- **БД Backup**: Используйте volume для PostgreSQL для сохранения данных
- **CORS**: Открыт для всех источников во всех средах (демо-настройка)
- **Swagger**: Включён во всех средах, включая Production

## 📈 Будущие улучшения

- Интеграция с реальным Steam API для автоматического сбора данных
- ClickHouse для глубокой аналитики
- GraphQL API в дополнение к REST
- Кэширование результатов (Redis)
- Фоновые задачи для синхронизации (Hangfire)
- WebSocket для real-time уведомлений

## 📝 Лицензия

[MIT](LICENSE)

## 👨‍💻 Автор

[SourceSurfer](https://github.com/SourceSurfer)

---

**Для вопросов и проблем**: обратитесь к документации API в Swagger UI
