-- Dev seed. Safe to drop in prod builds.
INSERT INTO hotels (name, city, country_code, address, star_rating, timezone) VALUES
  ('Sedra Grand',   'Amman',  'JO', '12 Rainbow St',    5, 'Asia/Amman'),
  ('Harbour Suites','Dubai',  'AE', '400 Marina Walk',  4, 'Asia/Dubai');

INSERT INTO room_types (hotel_id, code, name, max_occupancy, base_price, currency) VALUES
  (1, 'STD', 'Standard King', 2, 120.00, 'USD'),
  (1, 'DLX', 'Deluxe Twin',   3, 190.00, 'USD'),
  (2, 'STD', 'Standard King', 2,  95.00, 'USD');

INSERT INTO rooms (hotel_id, room_type_id, room_number, floor) VALUES
  (1, 1, '101', 1), (1, 1, '102', 1), (1, 2, '201', 2),
  (2, 3, '301', 3), (2, 3, '302', 3);

INSERT INTO guests (email, full_name, phone) VALUES
  ('demo.guest@example.com', 'Demo Guest', '+962700000000');
