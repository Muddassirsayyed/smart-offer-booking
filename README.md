# Smart Offer Slot Booking System

A production-level full-stack web application that allows businesses (gyms, salons, restaurants, clinics, coaching centers, gaming zones, turf owners) to create limited-time offers with slot booking functionality.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Tailwind CSS v4, Framer Motion, Recharts |
| Backend | .NET 8 Web API, Entity Framework Core, JWT Auth |
| Database | PostgreSQL |
| Auth | JWT Bearer Tokens |
| Docs | Swagger / OpenAPI |

---

## Project Structure

```
willovate_Hackathon project/
├── backend/
│   └── SmartOfferBooking/
│       ├── Controllers/          # Auth, Business, Offers, Slots, Bookings, Dashboard
│       ├── Data/                 # AppDbContext + EF Core config
│       ├── DTOs/                 # Request/Response DTOs
│       ├── Middleware/           # Global exception handler
│       ├── Models/               # Domain models (User, Business, Offer, Slot, Booking)
│       ├── Services/             # Business logic layer
│       ├── appsettings.json
│       └── Program.cs
├── frontend/
│   └── smart-offer-frontend/
│       └── src/
│           ├── api/              # Axios instance + API calls
│           ├── components/
│           │   ├── layout/       # AdminLayout, PublicLayout
│           │   ├── shared/       # BookingModal, ProtectedRoute
│           │   └── ui/           # Button, Card, Input, Modal, Badge, CountdownTimer
│           ├── context/          # AuthContext, ThemeContext
│           ├── pages/
│           │   ├── admin/        # Dashboard, Offers, Slots, Bookings, BusinessProfile
│           │   └── public/       # OfferListing, OfferDetail, BookingConfirmation
│           ├── types/            # TypeScript interfaces
│           └── utils/            # Helpers, constants
└── database/
    └── schema.sql                # PostgreSQL schema + seed data
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

---

## Setup Instructions

### 1. Database

```bash
# Create the database
psql -U postgres -c "CREATE DATABASE smart_offer_booking;"

# Or run the schema manually
psql -U postgres -d smart_offer_booking -f database/schema.sql
```

### 2. Backend

```bash
cd backend/SmartOfferBooking

# Copy and configure environment
cp appsettings.json appsettings.Development.json
# Edit appsettings.Development.json with your DB credentials and JWT key

# Restore packages
dotnet restore

# Run EF Core migrations (creates tables + seeds admin user)
dotnet ef migrations add InitialCreate
dotnet ef database update

# Start the API
dotnet run
# API runs at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### 3. Frontend

```bash
cd frontend/smart-offer-frontend

# Install dependencies (already done)
npm install

# Start dev server
npm run dev
# App runs at: http://localhost:5173
```

---

## Environment Variables

### Backend — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=smart_offer_booking;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SmartOfferBooking",
    "Audience": "SmartOfferBookingUsers"
  }
}
```

### Frontend — `.env` (optional, proxy is configured in vite.config.ts)

```env
VITE_API_BASE_URL=http://localhost:5000
```

---

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@smartoffer.com | Admin@123 |

---

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/register` | Register new admin |

### Business
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/business/my` | ✅ | Get my business |
| POST | `/api/business` | ✅ | Create business |
| PUT | `/api/business` | ✅ | Update business |

### Offers
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/offers` | ❌ | Public offer listing (with filters) |
| GET | `/api/offers/{id}` | ❌ | Get offer by ID |
| GET | `/api/offers/my` | ✅ | Get my offers |
| POST | `/api/offers` | ✅ | Create offer |
| PUT | `/api/offers/{id}` | ✅ | Update offer |
| DELETE | `/api/offers/{id}` | ✅ | Delete offer |

### Slots
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/slots/offer/{offerId}` | ❌ | Get slots for offer |
| POST | `/api/slots/offer/{offerId}` | ✅ | Create slot |
| PUT | `/api/slots/{id}` | ✅ | Update slot |
| DELETE | `/api/slots/{id}` | ✅ | Delete slot |

### Bookings
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/bookings` | ❌ | Create booking |
| GET | `/api/bookings/reference/{ref}` | ❌ | Get by reference |
| GET | `/api/bookings/my` | ✅ | Get business bookings |
| PUT | `/api/bookings/{id}/status` | ✅ | Update booking status |
| GET | `/api/bookings/export/csv` | ✅ | Export CSV |

### Dashboard
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/dashboard/summary` | ✅ | Dashboard analytics |

---

## Features

### Admin Portal
- 📊 Dashboard with charts (booking trend, category breakdown, capacity bar)
- 🏢 Business profile management
- 🏷️ Full offer CRUD (create, edit, delete, status management)
- 📅 Slot management with calendar view
- 📋 Booking management with status updates
- 📥 CSV export of bookings
- 🔍 Search and filter bookings

### Public Site
- 🔍 Browse and filter offers (by type, category, date, price, availability)
- ⏱️ Live countdown timers on offer cards
- 📄 Detailed offer page with slot selection
- 📝 Booking form with full validation
- ✅ Booking confirmation page with QR code
- 🔗 Shareable booking confirmation link

### Bonus Features
- 🌙 Dark / Light mode toggle
- 📱 Fully responsive mobile-first design
- ⏳ Loading skeletons
- 🔔 Toast notifications
- 🎫 QR code generation for bookings
- ⏰ Waitlist system
- 📊 Recharts analytics dashboard
- 🔐 JWT authentication with persistent login
- 🛡️ Protected admin routes
- 🌐 Global error handling middleware

---

## Build for Production

```bash
# Frontend
cd frontend/smart-offer-frontend
npm run build
# Output in dist/

# Backend
cd backend/SmartOfferBooking
dotnet publish -c Release -o ./publish
```
