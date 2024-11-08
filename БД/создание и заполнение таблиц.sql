create database [$ЯремаЗелепугин]

--должности
CREATE TABLE UserTypes (
    UserTypeID INT PRIMARY KEY IDENTITY(1,1),
    UserTypeName NVARCHAR(50) NOT NULL
);

--статусы
CREATE TABLE Statuses (
    StatusID INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(100) NOT NULL
);

-- Запчасти
CREATE TABLE Parts (
    RepairPartID INT PRIMARY KEY IDENTITY(1,1),
    RepairPartName NVARCHAR(100) NOT NULL
);

-- пользователи
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15) NOT NULL,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    UserTypeID INT NOT NULL,
    FOREIGN KEY (UserTypeID) REFERENCES UserTypes(UserTypeID)
);

-- заявки
CREATE TABLE Requests (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    StartDate DATETIME NOT NULL,
    ProblemDescription NVARCHAR(255) NOT NULL,
    RequestStatusID INT NOT NULL,
    CompletionDate DATETIME NULL,
    RepairPartID INT NULL,
    MasterID INT NOT NULL,
    ClientID INT NOT NULL,
    FOREIGN KEY (RequestStatusID) REFERENCES Statuses(StatusID),  
    FOREIGN KEY (RepairPartID) REFERENCES Parts(RepairPartID),   
    FOREIGN KEY (MasterID) REFERENCES Users(UserID),              
    FOREIGN KEY (ClientID) REFERENCES Users(UserID)               
);

-- комментарии
CREATE TABLE Comments (
    CommentID INT PRIMARY KEY IDENTITY(1,1),
    Message NVARCHAR(255) NOT NULL,
    MasterID INT NOT NULL,
    RequestID INT NOT NULL,
    FOREIGN KEY (MasterID) REFERENCES Users(UserID),              
    FOREIGN KEY (RequestID) REFERENCES Requests(RequestID)       
);

-- Модели
CREATE TABLE Models (
  ModelID INT PRIMARY KEY IDENTITY(1,1),
  orgTechType NVARCHAR(50) NOT NULL,
  orgTechModel NVARCHAR(100) NOT NULL,
  UNIQUE (orgTechType, orgTechModel) 
);

-- Таблица статусов
INSERT INTO Statuses (StatusName) VALUES
('Новая заявка'),
('В процессе ремонта'),
('Готова к выдаче');

-- Вставка данных в таблицу UserTypes
INSERT INTO UserTypes (UserTypeName) VALUES
('Заказчик'),
('Оператор'),
('Менеджер'),
('Мастер');

-- Вставка данных в таблицу Comments 
INSERT INTO Comments (Message, MasterID, RequestID) VALUES
 ('Интересно...', 2, 2), 
 ('Будем разбираться!', 3, 3), 
 ('Сделаем всё на высшем уровне!', 3, 4); 

-- Заполняем таблицу Requests 
INSERT INTO Requests (StartDate, ProblemDescription, RequestStatusID, CompletionDate, RepairPartID, MasterID, ClientID) VALUES
  ('2023-06-06', 'Перестал работать', (SELECT StatusID FROM Statuses WHERE StatusName = 'В процессе ремонта'), NULL, NULL, 2, 7),
  ('2023-05-05', 'Перестал работать', (SELECT StatusID FROM Statuses WHERE StatusName = 'В процессе ремонта'), NULL, NULL, 3, 7),
  ('2022-07-07', 'Не морозит одна из камер холодильника', (SELECT StatusID FROM Statuses WHERE StatusName = 'Готова к выдаче'), '2023-01-01', NULL, 2, 8),
  ('2023-08-02', 'Перестали работать многие режимы стирки', (SELECT StatusID FROM Statuses WHERE StatusName = 'Новая заявка'), NULL, NULL, 1, 8),
  ('2023-08-02', 'Перестала включаться', (SELECT StatusID FROM Statuses WHERE StatusName = 'Новая заявка'), NULL, NULL, 1, 9),
  ('2023-08-02', 'Перестал работать', (SELECT StatusID FROM Statuses WHERE StatusName = 'Готова к выдаче'), '2023-08-03', NULL, 2, 7),
  ('2023-07-09', 'Гудит, но не замораживает', (SELECT StatusID FROM Statuses WHERE StatusName = 'Готова к выдаче'), '2023-08-03', 
   (SELECT RepairPartID FROM Parts WHERE RepairPartName = 'Мотор обдува морозильной камеры холодильника'), 2, 8); 

-- Заполнение таблицы Users 
INSERT INTO Users (FullName, Phone, Login, Password, UserTypeID) VALUES
  ('Носов Иван Михайлович', '89210563128', 'login1', 'pass1', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Менеджер')),
  ('Ильин Александр Андреевич', '89535078985', 'login2', 'pass2', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Мастер')),
  ('Никифоров Иван Дмитриевич', '89210673849', 'login3', 'pass3', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Мастер')),
  ('Елисеев Артём Леонидович', '89990563748', 'login4', 'pass4', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Оператор')),
  ('Титов Сергей Кириллович', '89994563847', 'login5', 'pass5', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Оператор')),
  ('Григорьев Семён Викторович', '89219567849', 'login11', 'pass11', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Заказчик')),
  ('Сорокин Дмитрий Ильич', '89219567841', 'login12', 'pass12', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Заказчик')),
  ('Белоусов Егор Ярославович', '89219567842', 'login13', 'pass13', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Заказчик')),
  ('Суслов Михаил Александрович', '89219567843', 'login14', 'pass14', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Заказчик')),
  ('Васильев Вячеслав Александрович', '89219567844', 'login15', 'pass15', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = 'Мастер'));

      ALTER TABLE Requests
      ADD ModelID INT; 
      

-- Добавление столбца ModelID в таблицу Requests
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Requests') AND name = 'ModelID')
BEGIN
    ALTER TABLE Requests
    ADD ModelID INT;
END;

-- Создание внешнего ключа
ALTER TABLE Requests
ADD CONSTRAINT FK_Requests_Models FOREIGN KEY (ModelID) REFERENCES Models(ModelID);

--Вставка данных в модели
insert into Models values 
('Компьютер','DEXP Aquilon O286'),
('Компьютер','DEXP Atlas H388'),
('Ноутбук','MSI GF76 Katana 11UC-879XPU черный'),
('Ноутбук','MSI Modern 15 B12M-211RU черный'),
('Принтер','HP LaserJet Pro M404dn');

--Таблица Parts пустая
