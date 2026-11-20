-- ============================================================
-- Конноспортивный клуб и ипподром «Аллюр» (Вариант 22, ПР-23)
-- База данных MySQL/MariaDB: 7 таблиц, 3НФ, FK с RESTRICT/CASCADE
-- Импорт: phpMyAdmin -> SQL -> выбрать файл -> Вперёд
-- ============================================================

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

-- ------------------------------------------------------------
-- Структура таблицы `club_riders`
-- ------------------------------------------------------------
CREATE TABLE `club_riders` (
  `id` int(11) NOT NULL,
  `full_name` varchar(150) NOT NULL,
  `phone` varchar(20) NOT NULL,
  `skill_level` varchar(50) DEFAULT NULL,
  `discount_rate` decimal(5,2) NOT NULL DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `horses`
-- ------------------------------------------------------------
CREATE TABLE `horses` (
  `id` int(11) NOT NULL,
  `breed_id` int(11) NOT NULL,
  `nickname` varchar(100) NOT NULL,
  `passport_chip_code` varchar(50) NOT NULL,
  `birth_year` year(4) NOT NULL,
  `suit_color` varchar(50) DEFAULT NULL,
  `is_ready_for_training` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `horse_breeds`
-- ------------------------------------------------------------
CREATE TABLE `horse_breeds` (
  `id` int(11) NOT NULL,
  `breed_name` varchar(100) NOT NULL,
  `temperament` varchar(100) DEFAULT NULL,
  `origin_country` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `riding_orders`
-- ------------------------------------------------------------
CREATE TABLE `riding_orders` (
  `id` int(11) NOT NULL,
  `order_num` varchar(30) NOT NULL,
  `user_id` int(11) NOT NULL,
  `club_rider_id` int(11) NOT NULL,
  `order_date` datetime NOT NULL DEFAULT current_timestamp(),
  `total_price` decimal(10,2) NOT NULL DEFAULT 0.00,
  `status` enum('Забронировано','Лошадь оседлана','Тренировка в манеже','Завершена успешно','Отмена по ветпоказаниям') NOT NULL DEFAULT 'Забронировано',
  `cancel_reason` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `riding_session_items`
-- ------------------------------------------------------------
CREATE TABLE `riding_session_items` (
  `id` int(11) NOT NULL,
  `order_id` int(11) NOT NULL,
  `horse_id` int(11) NOT NULL,
  `session_datetime` datetime NOT NULL,
  `duration_minutes` int(11) NOT NULL DEFAULT 45,
  `has_trainer` tinyint(1) NOT NULL DEFAULT 0,
  `item_cost` decimal(10,2) NOT NULL DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `roles`
-- ------------------------------------------------------------
CREATE TABLE `roles` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Структура таблицы `users`
-- ------------------------------------------------------------
CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `role_id` int(11) NOT NULL,
  `login` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `full_name` varchar(150) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ------------------------------------------------------------
-- Демонстрационные данные (5-10 записей в каждой таблице)
-- Пароли (SHA256): bereitor123 / admin123 / manager123
-- ------------------------------------------------------------
INSERT INTO `roles` (`id`, `name`) VALUES
(1, 'Главный берейтор-зоотехник'),
(2, 'Администратор манежа');

INSERT INTO `users` (`id`, `role_id`, `login`, `password_hash`, `full_name`) VALUES
(1, 1, 'bereitor', '688cc62f2bb0a5ce8f95363cec65f7c4321cb0ce42a61854169eeaa9efff1310', 'Смирнов Алексей Петрович'),
(2, 2, 'admin',    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Кузнецова Мария Ивановна'),
(3, 2, 'manager',  '866485796cfa8d7c0cf7111640205b83076433547577511d81f8030ae99ecea5', 'Волков Дмитрий Сергеевич');

INSERT INTO `horse_breeds` (`id`, `breed_name`, `temperament`, `origin_country`) VALUES
(1, 'Орловский рысак', 'Спокойный', 'Россия'),
(2, 'Арабская чистокровная', 'Горячая', 'ОАЭ'),
(3, 'Владимирский тяжеловоз', 'Добродушный', 'Россия'),
(4, 'Тракененская', 'Энергичная', 'Россия');

INSERT INTO `horses` (`id`, `breed_id`, `nickname`, `passport_chip_code`, `birth_year`, `suit_color`, `is_ready_for_training`) VALUES
(1, 1, 'Гром',    '985112345678901', 2019, 'Вороной',      1),
(2, 2, 'Заря',    '985112345678902', 2017, 'Гнедая',       1),
(3, 3, 'Ветер',   '985112345678903', 2020, 'Рыжий',        1),
(4, 4, 'Молния',  '985112345678904', 2018, 'Тёмно-гнедая', 0),
(5, 1, 'Снежок',  '985112345678905', 2021, 'Серый',        1),
(6, 2, 'Песня',   '985112345678906', 2016, 'Рыжая',        1);

INSERT INTO `club_riders` (`id`, `full_name`, `phone`, `skill_level`, `discount_rate`) VALUES
(1, 'Анна Соколова',   '+7(921)555-01-01', 'Опытный',     10.00),
(2, 'Игорь Лебедев',   '+7(921)555-01-02', 'Начинающий',   0.00),
(3, 'Ольга Морозова',  '+7(921)555-01-03', 'Опытный',     15.00),
(4, 'Павел Орлов',     '+7(921)555-01-04', 'Любитель',     5.00),
(5, 'Елена Быстрова',  '+7(921)555-01-05', 'Опытный',     10.00);

INSERT INTO `riding_orders` (`id`, `order_num`, `user_id`, `club_rider_id`, `order_date`, `total_price`, `status`, `cancel_reason`) VALUES
(1, 'ПР23-КСК-2026-00001', 2, 1, '2026-11-21 10:00:00', 4950.00, 'Забронировано', NULL),
(2, 'ПР23-КСК-2026-00002', 2, 2, '2026-11-21 12:00:00', 1500.00, 'Лошадь оседлана', NULL),
(3, 'ПР23-КСК-2026-00003', 1, 3, '2026-11-20 09:00:00', 2125.00, 'Тренировка в манеже', NULL),
(4, 'ПР23-КСК-2026-00004', 2, 4, '2026-11-19 11:00:00', 3800.00, 'Завершена успешно', NULL),
(5, 'ПР23-КСК-2026-00005', 1, 5, '2026-11-18 15:00:00', 1350.00, 'Отмена по ветпоказаниям', 'Лошадь заболела, требуется ветеринарный осмотр'),
(6, 'ПР23-КСК-2026-00006', 2, 1, '2026-11-22 10:00:00', 3600.00, 'Забронировано', NULL);

INSERT INTO `riding_session_items` (`id`, `order_id`, `horse_id`, `session_datetime`, `duration_minutes`, `has_trainer`, `item_cost`) VALUES
(1, 1, 1, '2026-11-21 10:00:00', 45, 1, 2250.00),
(2, 1, 2, '2026-11-21 11:00:00', 90, 0, 2700.00),
(3, 2, 3, '2026-11-21 12:00:00', 45, 0, 1500.00),
(4, 3, 1, '2026-11-20 09:00:00', 45, 1, 2125.00),
(5, 4, 5, '2026-11-19 11:00:00', 90, 1, 3800.00),
(6, 5, 4, '2026-11-18 15:00:00', 45, 0, 1350.00),
(7, 6, 6, '2026-11-22 10:00:00', 45, 1, 2250.00),
(8, 6, 1, '2026-11-22 11:00:00', 45, 0, 1350.00);

-- ------------------------------------------------------------
-- Индексы сохранённых таблиц
-- ------------------------------------------------------------
ALTER TABLE `club_riders` ADD PRIMARY KEY (`id`);
ALTER TABLE `horses` ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `passport_chip_code` (`passport_chip_code`), ADD KEY `fk_horses_breed` (`breed_id`);
ALTER TABLE `horse_breeds` ADD PRIMARY KEY (`id`);
ALTER TABLE `riding_orders` ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `order_num` (`order_num`), ADD KEY `fk_orders_user` (`user_id`), ADD KEY `fk_orders_rider` (`club_rider_id`);
ALTER TABLE `riding_session_items` ADD PRIMARY KEY (`id`), ADD KEY `fk_items_order` (`order_id`), ADD KEY `fk_items_horse` (`horse_id`);
ALTER TABLE `roles` ADD PRIMARY KEY (`id`);
ALTER TABLE `users` ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `login` (`login`), ADD KEY `fk_users_role` (`role_id`);

-- ------------------------------------------------------------
-- AUTO_INCREMENT для сохранённых таблиц
-- ------------------------------------------------------------
ALTER TABLE `club_riders` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `horses` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `horse_breeds` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `riding_orders` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `riding_session_items` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `roles` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `users` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

-- ------------------------------------------------------------
-- Ограничения внешнего ключа сохраненных таблиц
-- ------------------------------------------------------------
ALTER TABLE `horses`
  ADD CONSTRAINT `fk_horses_breed` FOREIGN KEY (`breed_id`) REFERENCES `horse_breeds` (`id`) ON UPDATE CASCADE;
ALTER TABLE `riding_orders`
  ADD CONSTRAINT `fk_orders_rider` FOREIGN KEY (`club_rider_id`) REFERENCES `club_riders` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON UPDATE CASCADE;
ALTER TABLE `riding_session_items`
  ADD CONSTRAINT `fk_items_order` FOREIGN KEY (`order_id`) REFERENCES `riding_orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_items_horse` FOREIGN KEY (`horse_id`) REFERENCES `horses` (`id`) ON UPDATE CASCADE;
ALTER TABLE `users`
  ADD CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
