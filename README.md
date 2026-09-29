# Steam Release Analytics API

[![CI](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain) [![Tests](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FSourceSurfer%2FSteamReleaseAnalytics%2Fbadges%2Ftests.json)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain)

Backend-сервис для сбора, агрегации и анализа данных о релизах игр на Steam. Приложение предоставляет REST API для получения информации о будущих релизах, статистики по жанрам и анализа динамики изменений.

## 🎯 Основные возможности

- **Календарь релизов** - получение игр по месяцам с группировкой по дням
- **Аналитика жанров** - топ-5 популярных жанров с статистикой
- **Анализ динамики** - отслеживание изменений популярности жанров за 3 месяца
- **JWT аутентификация** - защита критических эндпоинтов
- **REST API** - стандартный API для интеграции с фронтенд-приложениями
- **Swagger документация** - интерактивная документация API

## 🛠️ Технологический стек

- **Backend**: C# и ASP.NET Core 8
- **ORM**: Entity Framework Core 8.0.4
- **Database**: PostgreSQL 16 (основная БД), ClickHouse (для аналитики)
- **Аутентификация**: JWT токены
- **Контейнеризация**: Docker и Docker Compose
- **API документация**: Swagger / OpenAPI
- **Архитектура**: Микросервисная архитектура (расширяемость), модульная структура

## 📋 Структура проекта

```
SteamReleaseAnalytics/
├── SteamReleaseAnalytics.Api/           # REST API и контроллеры
│   ├── Controllers/
│   │   ├── GamesController.cs           # Эндпоинты для работы с играми
│   │   ├── AnalyticsController.cs       # Эндпоинты аналитики
│   │   └── AuthController.cs            # Аутентификация
│   ├── Program.cs                       # Конфигурация приложения
│   └── appsettings.json                 # Настройки
├── SteamReleaseAnalytics.Core/          # Бизнес-логика и модели
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
│   ├── Migrations/                      # Миграции БД
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
├── docker-compose.yml                   # Docker Compose конфигурация
├── Dockerfile                           # Docker образ для API
└── README.md                            # Документация
```

## 🚀 Быстрый старт

### Требования

- Docker Desktop (или Docker + Docker Compose)
- .NET 8 SDK (для локальной разработки)
- PostgreSQL 16 (если запускать без Docker)

### Запуск через Docker Compose

1. **Перейдите в папку проекта**:
```bash
cd SteamReleaseAnalytics
```

2. **Запустите контейнеры**:
```bash
docker-compose up --build
```

Это автоматически:
- Запустит PostgreSQL базу данных
- Запустит pgAdmin для управления БД
- Соберёт и запустит API приложение
- Создаст все необходимые таблицы

3. **Откройте приложение**:
- **API и Swagger**: http://localhost:8080/swagger/index.html
- **pgAdmin**: http://localhost:5050 (admin@example.com / admin)
- **PostgreSQL**: localhost:5432 (postgres / postgres)

### Запуск локально (без Docker)

1. **Установите зависимости**:
```bash
dotnet restore
```

2. **Создайте и примените миграции**:
```bash
dotnet ef database update --project SteamReleaseAnalytics.Infrastructure --startup-project SteamReleaseAnalytics.Api
```

3. **Запустите API**:
```bash
dotnet run --project SteamReleaseAnalytics.Api
```

API будет доступна на: https://localhost:7205/swagger/index.html

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

API использует JWT токены для защиты критических операций (POST, DELETE).

1. Получите токен через `/api/v1/auth/login`
2. Добавьте токен в заголовок Authorization:
```
Authorization: Bearer <ваш_токен>
```

Токен действителен 60 минут (настраивается в `appsettings.json`).

## 🗄️ База данных

### Таблицы

- **Games** - основная таблица с информацией об играх
- **Tags** - теги/жанры игр
- **GameTags** - связь между играми и тегами (M:N)
- **GameSnapshots** - исторические данные для анализа динамики

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
curl http://localhost:8080/api/v1/analytics/top-genres?month=11&year=2025
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

## 🚨 Важные замечания

- **Среда**: Production в Docker (без HTTPS для упрощения)
- **Безопасность**: В Production используйте настоящие сертификаты HTTPS
- **Секрет JWT**: Измените `JwtSettings.Secret` на длинный, случайный ключ
- **БД Backup**: Используйте volume для PostgreSQL для сохранения данных
- **CORS**: Настроен на все источники для разработки

## 📈 Будущие улучшения

- Интеграция с реальным Steam API для автоматического сбора данных
- ClickHouse для глубокой аналитики
- GraphQL API в дополнение к REST
- Кэширование результатов (Redis)
- Фоновые задачи для синхронизации (Hangfire)
- WebSocket для real-time уведомлений

## 📝 Лицензия

MIT

## 👨‍💻 Автор

Steam Release Analytics Backend - Тестовое задание

---

**Для вопросов и проблем**: обратитесь к документации API в Swagger UI