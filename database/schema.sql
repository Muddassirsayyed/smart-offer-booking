-- Smart Offer Slot Booking System - PostgreSQL Schema

CREATE DATABASE smart_offer_booking;
\c smart_offer_booking;

CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(200) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Phone" VARCHAR(20) DEFAULT '',
    "Role" VARCHAR(20) NOT NULL DEFAULT 'Customer',
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_users_email ON "Users"("Email");

CREATE TABLE "Businesses" (
    "Id" SERIAL PRIMARY KEY,
    "BusinessName" VARCHAR(200) NOT NULL,
    "BusinessType" VARCHAR(100) NOT NULL,
    "OwnerName" VARCHAR(100) NOT NULL,
    "Phone" VARCHAR(20) DEFAULT '',
    "Email" VARCHAR(200) DEFAULT '',
    "Address" VARCHAR(500) DEFAULT '',
    "City" VARCHAR(100) DEFAULT '',
    "LogoUrl" TEXT,
    "OpeningTime" TIME NOT NULL DEFAULT '06:00',
    "ClosingTime" TIME NOT NULL DEFAULT '22:00',
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UserId" INT NOT NULL UNIQUE REFERENCES "Users"("Id") ON DELETE CASCADE
);
CREATE INDEX idx_businesses_userid ON "Businesses"("UserId");
CREATE INDEX idx_businesses_type ON "Businesses"("BusinessType");

CREATE TABLE "Offers" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000) DEFAULT '',
    "Category" VARCHAR(100) DEFAULT '',
    "OriginalPrice" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "OfferPrice" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "DiscountPercentage" INT NOT NULL DEFAULT 0,
    "StartDate" TIMESTAMPTZ NOT NULL,
    "EndDate" TIMESTAMPTZ NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "Capacity" INT NOT NULL DEFAULT 10,
    "BookingLimit" INT NOT NULL DEFAULT 1,
    "Terms" VARCHAR(2000) DEFAULT '',
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Draft',
    "ImageUrl" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "BusinessId" INT NOT NULL REFERENCES "Businesses"("Id") ON DELETE CASCADE
);
CREATE INDEX idx_offers_businessid ON "Offers"("BusinessId");
CREATE INDEX idx_offers_status ON "Offers"("Status");
CREATE INDEX idx_offers_enddate ON "Offers"("EndDate");

CREATE TABLE "OfferSlots" (
    "Id" SERIAL PRIMARY KEY,
    "SlotDate" DATE NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "Capacity" INT NOT NULL DEFAULT 10,
    "BookedCount" INT NOT NULL DEFAULT 0,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Active',
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "OfferId" INT NOT NULL REFERENCES "Offers"("Id") ON DELETE CASCADE,
    CONSTRAINT chk_booked_count CHECK ("BookedCount" >= 0),
    CONSTRAINT chk_capacity CHECK ("Capacity" > 0),
    CONSTRAINT chk_booked_lte_capacity CHECK ("BookedCount" <= "Capacity")
);
CREATE INDEX idx_slots_offerid ON "OfferSlots"("OfferId");
CREATE INDEX idx_slots_date ON "OfferSlots"("SlotDate");

CREATE TABLE "Bookings" (
    "Id" SERIAL PRIMARY KEY,
    "BookingReference" VARCHAR(20) NOT NULL UNIQUE,
    "CustomerName" VARCHAR(100) NOT NULL,
    "CustomerPhone" VARCHAR(20) NOT NULL,
    "CustomerEmail" VARCHAR(200) NOT NULL,
    "NumberOfPeople" INT NOT NULL DEFAULT 1,
    "SpecialNote" VARCHAR(500) DEFAULT '',
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Pending',
    "QrCodeData" TEXT,
    "IsWaitlisted" BOOLEAN NOT NULL DEFAULT FALSE,
    "WaitlistPosition" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "SlotId" INT NOT NULL REFERENCES "OfferSlots"("Id") ON DELETE CASCADE,
    CONSTRAINT chk_people CHECK ("NumberOfPeople" > 0)
);
CREATE INDEX idx_bookings_reference ON "Bookings"("BookingReference");
CREATE INDEX idx_bookings_slotid ON "Bookings"("SlotId");
CREATE INDEX idx_bookings_email ON "Bookings"("CustomerEmail");
CREATE INDEX idx_bookings_status ON "Bookings"("Status");

CREATE TABLE "NotificationLogs" (
    "Id" SERIAL PRIMARY KEY,
    "Type" VARCHAR(20) NOT NULL,
    "Recipient" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Sent',
    "SentAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "BookingId" INT NOT NULL REFERENCES "Bookings"("Id") ON DELETE CASCADE
);
CREATE INDEX idx_notif_bookingid ON "NotificationLogs"("BookingId");

-- =============================================
-- SEED DATA (use EF Core migrations for hashed password)
-- =============================================

-- Sample Offers (insert after running EF migrations which seed admin user + business)
INSERT INTO "Offers" ("Title","Description","Category","OriginalPrice","OfferPrice","DiscountPercentage","StartDate","EndDate","StartTime","EndTime","Capacity","BookingLimit","Terms","Status","BusinessId","CreatedAt","UpdatedAt")
VALUES
('Summer Fitness Package','Get fit this summer with our exclusive 3-month membership. Includes all equipment, group classes, and personal trainer sessions.','Fitness',5000,1999,60,NOW(),NOW()+INTERVAL '30 days','06:00','22:00',50,2,'Valid for 3 months. Non-transferable. No refunds after booking.','Active',1,NOW(),NOW()),
('Weekend Yoga Retreat','Rejuvenate your mind and body with our weekend yoga sessions. Suitable for all levels.','Wellness',2000,799,60,NOW(),NOW()+INTERVAL '14 days','07:00','09:00',20,1,'Bring your own yoga mat. Comfortable clothing required.','Active',1,NOW(),NOW()),
('Haircut & Styling Combo','Premium haircut, wash, and styling by expert stylists. Walk out looking your best.','Beauty',1500,599,60,NOW(),NOW()+INTERVAL '21 days','09:00','20:00',15,1,'Appointment required. 24hr cancellation policy.','Active',1,NOW(),NOW());

INSERT INTO "OfferSlots" ("SlotDate","StartTime","EndTime","Capacity","BookedCount","Status","OfferId","CreatedAt")
VALUES
(CURRENT_DATE+1,'06:00','08:00',20,5,'Active',1,NOW()),
(CURRENT_DATE+1,'08:00','10:00',20,12,'Active',1,NOW()),
(CURRENT_DATE+2,'06:00','08:00',20,0,'Active',1,NOW()),
(CURRENT_DATE+3,'07:00','09:00',15,8,'Active',2,NOW()),
(CURRENT_DATE+4,'07:00','09:00',15,3,'Active',2,NOW()),
(CURRENT_DATE+1,'10:00','11:00',10,4,'Active',3,NOW()),
(CURRENT_DATE+2,'14:00','15:00',10,0,'Active',3,NOW());
