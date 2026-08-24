-- Hotel Booking System - baseline schema
-- Runs once, on empty mysql-data volume.
SET NAMES utf8mb4;
SET time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS hotels (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  name          VARCHAR(160)  NOT NULL,
  city          VARCHAR(120)  NOT NULL,
  country_code  CHAR(2)       NOT NULL,
  address       VARCHAR(255)  NOT NULL,
  star_rating   TINYINT UNSIGNED NULL,
  timezone      VARCHAR(64)   NOT NULL DEFAULT 'UTC',
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  KEY idx_hotels_city (country_code, city),
  CONSTRAINT chk_hotels_star CHECK (star_rating IS NULL OR star_rating BETWEEN 1 AND 5)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS room_types (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  hotel_id      BIGINT UNSIGNED NOT NULL,
  code          VARCHAR(32)   NOT NULL,
  name          VARCHAR(120)  NOT NULL,
  max_occupancy TINYINT UNSIGNED NOT NULL,
  base_price    DECIMAL(10,2) NOT NULL,
  currency      CHAR(3)       NOT NULL DEFAULT 'USD',
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_room_types_hotel_code (hotel_id, code),
  CONSTRAINT fk_room_types_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE,
  CONSTRAINT chk_room_types_price CHECK (base_price >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS rooms (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  hotel_id      BIGINT UNSIGNED NOT NULL,
  room_type_id  BIGINT UNSIGNED NOT NULL,
  room_number   VARCHAR(16)   NOT NULL,
  floor         SMALLINT      NULL,
  status        ENUM('available','maintenance','out_of_service') NOT NULL DEFAULT 'available',
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_rooms_hotel_number (hotel_id, room_number),
  KEY idx_rooms_type (room_type_id, status),
  CONSTRAINT fk_rooms_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE CASCADE,
  CONSTRAINT fk_rooms_type  FOREIGN KEY (room_type_id) REFERENCES room_types(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS guests (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  email         VARCHAR(255)  NOT NULL,
  full_name     VARCHAR(160)  NOT NULL,
  phone         VARCHAR(32)   NULL,
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_guests_email (email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bookings (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  reference     CHAR(12)      NOT NULL,
  hotel_id      BIGINT UNSIGNED NOT NULL,
  guest_id      BIGINT UNSIGNED NOT NULL,
  room_id       BIGINT UNSIGNED NOT NULL,
  check_in      DATE          NOT NULL,
  check_out     DATE          NOT NULL,
  adults        TINYINT UNSIGNED NOT NULL DEFAULT 1,
  children      TINYINT UNSIGNED NOT NULL DEFAULT 0,
  total_amount  DECIMAL(10,2) NOT NULL,
  currency      CHAR(3)       NOT NULL DEFAULT 'USD',
  status        ENUM('pending','confirmed','checked_in','checked_out','cancelled','no_show')
                              NOT NULL DEFAULT 'pending',
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_bookings_reference (reference),
  KEY idx_bookings_room_dates (room_id, check_in, check_out),
  KEY idx_bookings_hotel_status (hotel_id, status, check_in),
  KEY idx_bookings_guest (guest_id),
  CONSTRAINT fk_bookings_hotel FOREIGN KEY (hotel_id) REFERENCES hotels(id) ON DELETE RESTRICT,
  CONSTRAINT fk_bookings_guest FOREIGN KEY (guest_id) REFERENCES guests(id) ON DELETE RESTRICT,
  CONSTRAINT fk_bookings_room  FOREIGN KEY (room_id)  REFERENCES rooms(id)  ON DELETE RESTRICT,
  CONSTRAINT chk_bookings_dates  CHECK (check_out > check_in),
  CONSTRAINT chk_bookings_amount CHECK (total_amount >= 0)
) ENGINE=InnoDB;

-- one row per room per night. UNIQUE = overbooking guard at DB level.
CREATE TABLE IF NOT EXISTS room_night_allocations (
  room_id     BIGINT UNSIGNED NOT NULL,
  stay_date   DATE            NOT NULL,
  booking_id  BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (room_id, stay_date),
  KEY idx_rna_booking (booking_id),
  CONSTRAINT fk_rna_room    FOREIGN KEY (room_id)    REFERENCES rooms(id)    ON DELETE CASCADE,
  CONSTRAINT fk_rna_booking FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS payments (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  booking_id    BIGINT UNSIGNED NOT NULL,
  provider      VARCHAR(48)   NOT NULL,
  provider_ref  VARCHAR(128)  NULL,
  amount        DECIMAL(10,2) NOT NULL,
  currency      CHAR(3)       NOT NULL DEFAULT 'USD',
  status        ENUM('initiated','authorized','captured','failed','refunded')
                              NOT NULL DEFAULT 'initiated',
  created_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_payments_provider_ref (provider, provider_ref),
  KEY idx_payments_booking (booking_id, status),
  CONSTRAINT fk_payments_booking FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE
) ENGINE=InnoDB;
