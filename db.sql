-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: learnhubdb
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `courses`
--

DROP TABLE IF EXISTS `courses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `courses` (
  `image_path` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `level` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Beginner',
  `id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `category` varchar(40) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `courses`
--

LOCK TABLES `courses` WRITE;
/*!40000 ALTER TABLE `courses` DISABLE KEYS */;
INSERT INTO `courses` VALUES ('/images/Web_dev.jpg','Beginner',5,'Web Foundation','web Development','beginner introduction for HTML for content, CSS for style, and pages that work for everyone.'),('/images/database.jpg','Beginner',6,'Database Introduction','Database','Introduction of database on what they are, how tables and keys work, and the four basic SQL commands.'),('/images/security.jpg','Beginner',7,'Introduction of Web Security','Web Security','Introduction of web security. Strong passwords, logging in safely, user roles and everyday online threats.'),('/images/js.jpg','Beginner',8,'Introduction of JavaScript','Javascript','Introduction of JavaScript. Storing values, reacting to clicks and searching through lists.');
/*!40000 ALTER TABLE `courses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `discussion_posts`
--

DROP TABLE IF EXISTS `discussion_posts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `discussion_posts` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `course_id` int NOT NULL,
  `body` varchar(2000) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  `updated_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  `parent_id` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_posts_parent` (`parent_id`),
  KEY `discussion_posts_ibfk_1` (`user_id`,`course_id`),
  CONSTRAINT `discussion_posts_ibfk_1` FOREIGN KEY (`user_id`, `course_id`) REFERENCES `enrollments` (`user_id`, `course_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_posts_parent` FOREIGN KEY (`parent_id`) REFERENCES `discussion_posts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `discussion_posts`
--

LOCK TABLES `discussion_posts` WRITE;
/*!40000 ALTER TABLE `discussion_posts` DISABLE KEYS */;
INSERT INTO `discussion_posts` VALUES (3,9,6,'dasuodousabdouisabfisaifsia','2026-10-27 10:40:08','2026-10-27 10:40:08',NULL);
/*!40000 ALTER TABLE `discussion_posts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `enrollments`
--

DROP TABLE IF EXISTS `enrollments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `enrollments` (
  `user_id` int NOT NULL,
  `course_id` int NOT NULL,
  `joined_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`user_id`,`course_id`),
  KEY `fk_enrollments_course` (`course_id`),
  CONSTRAINT `fk_enrollments_course` FOREIGN KEY (`course_id`) REFERENCES `courses` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_enrollments_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `enrollments`
--

LOCK TABLES `enrollments` WRITE;
/*!40000 ALTER TABLE `enrollments` DISABLE KEYS */;
INSERT INTO `enrollments` VALUES (9,5,'2026-10-26 05:27:05'),(9,6,'2026-10-26 06:35:16'),(9,7,'2026-10-26 07:00:07');
/*!40000 ALTER TABLE `enrollments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `lesson_notes`
--

DROP TABLE IF EXISTS `lesson_notes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lesson_notes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `lesson_id` int NOT NULL,
  `note_text` varchar(4000) COLLATE utf8mb4_unicode_ci NOT NULL,
  `updated_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_note` (`user_id`,`lesson_id`),
  KEY `lesson_id` (`lesson_id`),
  CONSTRAINT `lesson_notes_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `lesson_notes_ibfk_2` FOREIGN KEY (`lesson_id`) REFERENCES `lessons` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lesson_notes`
--

LOCK TABLES `lesson_notes` WRITE;
/*!40000 ALTER TABLE `lesson_notes` DISABLE KEYS */;
INSERT INTO `lesson_notes` VALUES (6,9,23,'dsadsadsadsadsa','2026-10-26 07:33:38');
/*!40000 ALTER TABLE `lesson_notes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `lesson_progress`
--

DROP TABLE IF EXISTS `lesson_progress`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lesson_progress` (
  `user_id` int NOT NULL,
  `lesson_id` int NOT NULL,
  `completed_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`user_id`,`lesson_id`),
  KEY `fk_progress_lesson` (`lesson_id`),
  CONSTRAINT `fk_progress_lesson` FOREIGN KEY (`lesson_id`) REFERENCES `lessons` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_progress_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lesson_progress`
--

LOCK TABLES `lesson_progress` WRITE;
/*!40000 ALTER TABLE `lesson_progress` DISABLE KEYS */;
INSERT INTO `lesson_progress` VALUES (9,21,'2026-10-26 07:33:29'),(9,22,'2026-10-26 07:33:33'),(9,23,'2026-10-26 07:33:34'),(9,24,'2026-10-26 07:33:31');
/*!40000 ALTER TABLE `lesson_progress` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `lessons`
--

DROP TABLE IF EXISTS `lessons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lessons` (
  `video_url` varchar(400) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `id` int NOT NULL AUTO_INCREMENT,
  `course_id` int NOT NULL,
  `title` varchar(120) COLLATE utf8mb4_unicode_ci NOT NULL,
  `content` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `position` int NOT NULL DEFAULT '1',
  `image_path` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `image_alt` varchar(300) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `ix_lessons_order` (`course_id`,`position`),
  CONSTRAINT `fk_lessons_course` FOREIGN KEY (`course_id`) REFERENCES `courses` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ck_lessons_position` CHECK ((`position` between 1 and 1000))
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lessons`
--

LOCK TABLES `lessons` WRITE;
/*!40000 ALTER TABLE `lessons` DISABLE KEYS */;
INSERT INTO `lessons` VALUES ('https://www.youtube-nocookie.com/embed/-Hh9DpgULHU',17,5,'What is a website?','A website is a group of pages that your web browser (like Chrome or Edge) downloads and displays on your screen. When you type in an address your browser asks a computer called a server for the page and the server sends it back to you.\r\n\r\nAlmost every web page consists of three languages. HTML is the content and structure: headings, paragraphs, images, links. CSS is the style: colors, fonts, spacing, layout. JavaScript is the behavior, the what happens when you click, type or scroll.\r\n\r\nThe easiest way to remember it is a house. HTML is like the walls and rooms, CSS is like the paint and furniture and for JavaScript is the electricity that makes things work.',1,'',''),('https://www.youtube-nocookie.com/embed/salY_Sm6mv4',18,5,'HTML basic','HTML uses tags to specify every piece of a page. Most tags are paired, having an opening tag (like <p>) and a closing tag (like </p>), with the content sandwiched in between.\r\n\r\nSome of the top tags include: <h1> to <h6> for headings, most important to least important. <p> for paragraph of text. <a> to link to another page. <img> tag for picture. And bulleted lists (<ul>, <li>).\r\n\r\nEach page has two main parts. The head has information about the page, like the title in the browser tab. The body contains everything you can see.\r\n\r\nGood pages also use tags describing their layout: <header> for the top of the page, <nav> for the menu, <main> for the main content and <footer> for the bottom. They help search engines and screen readers understand the page.',2,'',''),('https://www.youtube-nocookie.com/embed/OEV8gMkCHXQ',19,5,'CSS basics','CSS is the language that controls how your HTML looks. A CSS rule has two parts: a selector that picks out which elements to style, and declarations that tell how they should look.\r\n\r\nThis rule, for instance, makes every paragraph dark blue and a little bigger:\r\np { color:navy;font-size:18px; }\r\n\r\nThe elements can be selected in three major methods: tag name (p), class (.card), and id (#menu). Classes are the most valuable because they can be applied to many elements.\r\n\r\nThere are three locations where CSS can be included. The external CSS is kept in a separate .css file. This type of CSS is available for use in all pages. The internal CSS is inserted within the <style> tag of a page. The inline CSS is written directly on an element with the style property.\r\n\r\nCSS is also responsible for layout. The CSS Grid method organizes content in rows and columns, and the LearnHub home page is organized in such manner.',3,'/images/images.jpg','The diagram showing an illustration of a CSS rule, h1 { color: blue; }, highlighting the selector (h1), the property (color), the value (blue) and the declaration enclosed in the curly brackets.'),('https://www.youtube-nocookie.com/embed/oNSX21dh6kw',20,5,'Responsive Design','The concept of responsive design entails developing a webpage that can adapt to all kinds of screens, be it a large computer screen or a mobile screen. Rather than designing different websites for various devices, one web page changes its layout depending on which screen it is viewed on.\r\n\r\nA responsive website incorporates some simple methods:\r\n- The viewport meta tag: <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> makes sure that the website displays its true size instead of scaling out on the phone.\r\n- Flexible layouts incorporate the percentage measure, CSS Grid and Flexbox in place of pixels. In this way, the box sizes can fluctuate.\r\n- Flexible images utilize max-width: 100%. Images will not become wider than the screen size.\r\n- Media queries are used to add additional CSS styles on particular screen size conditions.\r\n\r\nFor example, there can be three product cards appearing on the laptop screen side by side while two appear on a tablet and just one appears on a mobile phone stacked vertically.',4,'/images/ResponsiveDesignDragram.png','Each HTML webpage undergoes media queries and therefore can be styled with mobile, tablet, or desktop CSS to fit all screens.'),('https://www.youtube-nocookie.com/embed/X7v0O8yiUuY',21,6,'What is a database?','A database is a structured repository of data that can be stored, searched for, and updated rapidly by the computer.\r\n\r\nDatabases are all around us. The contacts of our phones, the catalog of products in a retail store and even the record of students in a university are maintained in databases. When we log into a site using our email and password, we have accessed the database.\r\n\r\nDatabase management system is the software used to manage databases. One of the best free systems is MySQL. The other types are SQL Server, Postgres, and SQLite.\r\n\r\nIn order to communicate with the database, we use the Structured Query Language, or SQL. With SQL, we can make requests like \"give me all the courses\" or \"add this new user.\"',1,'',''),('',22,6,'Tables, rows and keys','Most of the database store information in tables, which look a lot like spreadsheets.\r\n\r\nEach table needs to have a primary key, which means that there will be a field that has a unique value in each row. Primary keys are often named id. The names of two students may be similar, but their ids cannot be the same because the id field allows the database to distinguish the records.\r\n\r\nTable relations are possible through foreign keys. Foreign keys represent fields that hold the id of some record from another table. As an example, there is an online platform for learning. In this case, each lesson has a course_id field which holds the id of its course.\r\n\r\nWhen data is divided into interrelated tables, it implies that each piece of information is entered only once. In the case of the change of a course name, it needs to be changed only once.',2,'',''),('',23,6,'Basic SQL: Create, Read, Update, Delete','The majority of operations performed on a web site using information fall into one of four categories called CRUD. Each category has its own command in SQL:\r\n- Creation \r\n- Reading\r\n- Updating\r\n- Deletion\r\n\r\nWHERE clause specifies which rows the query will affect. It is critical to both UPDATE and DELETE statements because otherwise all rows will be updated/deleted.\r\n\r\nSome of the additions to SELECT include WHERE that filters rows and ORDER BY that sorts rows. An example of SELECT statement is the following one: SELECT title FROM courses WHERE level = \'Beginner\' ORDER BY title; this statement will return the title column of all beginner courses ordered alphabetically.\r\n\r\nThe majority of website functionalities involve these commands. Registration uses an INSERT, viewing a products list uses a SELECT, editing user\'s profile uses an UPDATE and deleting a product from the cart uses a DELETE.',4,'/images/CRUD.png','The four CRUD operations: Create uses INSERT, Read uses SELECT, Update uses UPDATE and Delete uses DELETE.'),('',24,6,'Keeping data correct','Data should be accurate in order for a database to be effective. There are rules in place in databases that prevent errors from being made.\r\n\r\nNot Null requires data to be entered in all rows of a table. A user cannot exist without an email. Unique prevents duplicate data from existing in a row. Two different accounts cannot use the same email. Foreign Key guarantees that references point to valid data. A lesson must be attached to an existing course.\r\n\r\nSometimes there may be multiple steps which should work out in unison in order to accomplish the desired result. For instance, saving the outcome of a test involves not only storing the overall score, but also all of the answers given. The transaction includes all these steps in such a way that either everything is saved or nothing is.\r\n\r\nBefore the data gets into the database, websites make sure that the information is complete and valid. If there are some missing fields, a message will appear.',1,'',''),('https://www.youtube-nocookie.com/embed/kxWkteHG16c',25,7,'What is web security?','Website security refers to the process of securing the website as well as the users from any third party that wants to steal, alter or destroy information.\r\n\r\nAny website containing personal information has three major objectives which include:\r\n- Confidentiality: Data is accessible only to authorized persons. You are the only person that can view your test score.\r\n- Integrity: Unauthorized alterations of data are not allowed. No one is allowed to change your notes.\r\n- Availability: The website is operational when required.\r\n\r\nA simple example of security is HTTPS. The padlock shown in the browser URL signifies that the connection is encrypted and therefore any interception of the network traffic will not be able to decipher your password or message.\r\n\r\nSecurity does not only rely on technology. There are many ways by which the attackers use tricks to fool people into compromising security, such as email spoofing. This is why both the website and the person using it must be involved.\r\n\r\nIn this course, you will be introduced to password management, logging in, user roles, and various threats.',1,'',''),('',26,7,'Strong passwords and hashing','Given that passwords are the most widely used method to identify yourself online, they have to be secure and kept safe.\r\n\r\nThe best password is a long and random one. It should contain at least 10 characters, which have to be a combination of capital letters, lower case letters, digits, and punctuation marks, and should not be names, dates of birth, or popular words like \"password123\". Use a unique password for each website in order to avoid compromising your whole profile if the password gets stolen.\r\n\r\nWeb pages must not keep your actual password. What they keep is the hash. The hash is the scrambled password created through a one-way function. It is easy to hash the password, but it is impossible to crack it.\r\n\r\nWhen you log on, the site takes the information you entered and makes a hash of it, then checks it against the one stored. They have to match for your password to be considered correct. It\'s also important to include a random number called a salt.',2,'/images/passwordhash.png','A password and a random salt go through a password hasher, and only the resulting hash is stored.'),('',27,7,'Logging in and user roles','Logging in helps resolve two different questions.\r\n\r\nAuthentication is concerned with \"Who are you?\" Authentication requires that you log into the website using your email and password. Once both of these are verified correctly, the web site gives your browser a small file called the cookie, ensuring that you stay logged in through all pages on the site.\r\n\r\nAuthorization is concerned with \"What are you allowed to do?\" Every account on the site has a role. On a learning site, for example, the Member is allowed to join the courses, make notes, and contribute in discussions, while the Admin is additionally able to handle courses, users, and postings. The server checks your role before displaying the restricted page to you.\r\n\r\nWebsites should also keep each individual\'s personal information safe. When a note is edited, the website verifies that the note belongs to the individual, thereby making sure that no one can open up or edit anyone else’s notes by changing a digit in the URL.\r\n\r\nSome other best practices include locking an account for several minutes after several incorrect passwords have been entered and signing out from shared computers.',3,'/images/Authentication_Authorization.png','Authentication confirms users are who they say they are with a login form; authorization then gives them permission to access only certain resources.'),('https://www.youtube-nocookie.com/embed/Y7zNlEMDmI4',28,7,'Everyday online threats','The majority of website attacks and user attacks usually occur in several typical ways. These are important to know to protect yourself and make your website safer.\r\n\r\nPhishing is a fraudulent e-mail, message, or website that mimics the identity of your bank or university or any other organization you are a customer of, in order to get your password. Pay attention to the sender and the web address and never enter your password clicking an unexpected link.\r\n\r\nMalicious input refers to entering malicious code in a website form. If a website displayed comments just as they were entered, then some user could publish a script, which will run in every visitor\'s browser. Secure websites consider everything a user enters as just text. In other words, if you type <b>hello</b> in a comment in a secure website, then all other users will see the same text, not bold text.\r\n\r\nReusing weak passwords allows those who hack one password to gain access to multiple accounts.\r\n\r\nThere are several simple defenses that will protect from all but a few of these attacks: ensure your software is patched, use strong and unique passwords, be sure to validate your input on the server side.',4,'',''),('https://www.youtube-nocookie.com/embed/W6NZfCO5SIk',29,8,'What is JavaScript?','JavaScript is the programming language of the Web page. The HTML provides content to the page while the CSS gives style to it. But it is JavaScript that makes it interactive and it can respond to user actions like clicking, typing, scrolling and can refresh the page without reloading it.\r\n\r\nJavaScript can be found on almost any website you come across. It alerts about an improperly filled form, brings menus up, starts video, and performs other tasks like changing search results in real-time as you type.\r\n\r\nJavaScript executes on your computer within the browser. This makes it very fast because changes happen instantaneously. This also means that users can turn JavaScript off or modify it, which makes it necessary for sites to verify some operations on their servers.\r\n',1,'',''),('',30,8,'Variables and values','A variable is a named container that holds a value which can be accessed later.\r\n\r\nIn JavaScript, a variable is declared using either let or const.\r\nlet score = 0;\r\nconst courseName = \'Interactive JavaScript\';\r\n\r\nIf the value does not need to be changed ever after its declaration, you use const. If it needs changing after declaration, you use let. Then you can change the value of the variable like this: score = score + 1;\r\n\r\nHere are a few basic types of values:\r\n- Numbers: 4 and 3.5\r\n- Strings (texts): anything inside quotation marks, e.g., \'Hello\'\r\n- Booleans: true or false\r\n- Arrays: lists, like [\'HTML\', \'CSS\', \'SQL\']\r\n\r\nTo add text and values together, use a template string. It is enclosed in backticks with ${ } syntax.\r\nconst message = `You completed ${score} lessons`;\r\n\r\nChoose descriptive names for your variables, like lessonsCompleted, not x.',2,'',''),('',31,8,'Reacting to clicks','The language starts to be interesting only when it reacts to the actions of the user. User actions are referred to as events, including clicks, keystrokes, input into a field, and form submission.\r\n\r\nIn order to respond to an event, one needs to select the element first and attach an event listener, which is a callback function triggered every time an event occurs.\r\n\r\nconst button = document.querySelector(\'#hello\');\r\nconst message = document.querySelector(\'#message\');\r\nbutton.addEventListener(\'click\', () => {\r\nmessage.textContent = \'Thanks for clicking!\';\r\n});\r\n\r\nThe document.querySelector selects an element with the same selectors as CSS: #hello will find an element with id=\"hello\". The textContent property changes the text content of an element.\r\n\r\nExamples of common events are \'click\' for buttons, \'input\' for every change in the text field, and \'submit\' for form elements.',3,'/images/Javascript-Event-Listener.jpg','An event listener is code that listens for specific events in a web page, such as a click, and responds to them.'),('',32,8,'Working with lists','Lists are a common component used on websites. These could include course lists, product lists, and message lists. In JavaScript, lists are represented by arrays.\r\n\r\nconst courses = [\'Web Foundations\', \'Database Essentials\', \'Secure Web Applications\'];\r\n\r\nEach element is identified by its position index, which starts at 0; thus, courses[0] corresponds to \'Web Foundations\'. You can get the total number of elements in an array using courses.length.\r\n\r\nYou can iterate through all elements in a loop:\r\nfor (const course of courses) {\r\nconsole.log(course);\r\n}\r\n\r\nArrays come with several useful functions that can be utilized. The push() function appends an element at the end. The filter() function creates a list of elements that satisfy some condition. For instance, this will create an array of courses whose name contains the word \"Web\":\r\nconst webCourses = courses.filter(c => c.includes(\'Web\'));\r\n\r\nIn live search, filter is used with the input event.',4,'/images/javascript_array.jpg','A JavaScript array of five numbers, 2, 4, 8, 12 and 16, with each value stored at an index position from 0 to 4.');
/*!40000 ALTER TABLE `lessons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `questions`
--

DROP TABLE IF EXISTS `questions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `questions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `course_id` int NOT NULL,
  `prompt` varchar(400) COLLATE utf8mb4_unicode_ci NOT NULL,
  `option_a` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `option_b` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `option_c` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `correct_option` int NOT NULL,
  `explanation` varchar(800) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_questions_course` (`course_id`),
  CONSTRAINT `fk_questions_course` FOREIGN KEY (`course_id`) REFERENCES `courses` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ck_questions_option` CHECK ((`correct_option` between 0 and 2))
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `questions`
--

LOCK TABLES `questions` WRITE;
/*!40000 ALTER TABLE `questions` DISABLE KEYS */;
INSERT INTO `questions` VALUES (37,5,'Which language describes the content and structure of a web page?','HTML','CSS','SQL',0,'HTML describes the content and structure: headings, paragraphs, images and links.'),(38,5,'Which tag is used for a paragraph of text?','<h1>','<p>','<a>',1,'<p> marks a paragraph. <h1> is a main heading and <a> is a link.'),(39,5,'What does CSS control?','The data stored in a database','How a page looks, such as colours, fonts and layout','Which users can sign in',1,'CSS controls present the colour, fonts, spacing and layout.'),(40,5,'Where is external CSS written?','In its own css file shared by many pages','Inside a style attribute on one element','In the database',0,'External CSS lives in a separate css file, so one change updates every page that uses it'),(41,5,'What does a media query do?','Sends the page to a database','Applies CSS rules only when the screen is a certain size','Makes images load faster',1,'Media queries let one page change its layout on different screens, for example from three columns on a laptop to one column on a phone.'),(42,6,'What is a database?','A program for drawing pictures','An organised collection of information that can be stored and searched','A type of web browser',1,'A database stores information in an organised way so it can be found, added and changed quickly.'),(43,6,'In a database table, what is a row?','One record, such as one user','One detail about every record, such as email','The name of the table',0,'Each row is one record. Each column is one detail that every record has.'),(44,6,'What is a primary key?','A password for the database','The first column, always called key','A column whose value is different for every row',2,'A primary key, usually id, uniquely identifies each row, so the right record can always be found.'),(45,6,'Which SQL command shows existing data?','SELECT','INSERT','DELETE ',0,'SELECT reads and shows data. INSERT adds new rows and DELETE removes them.'),(46,6,'What does the UNIQUE rule on an email column do?',' Makes the email bold',' Stops two rows from having the same email ','Deletes old emails',1,'UNIQUE means no two rows can have the same value, so two accounts cannot share one email.'),(47,7,'What does the padlock in the browser address bar mean?','The website is free to use','The connection uses HTTPS and is encrypted ','You are signed in ',1,'HTTPS encrypts the connection, so others on the network cannot read your password or messages.'),(48,7,'Which of these is the strongest password?','password123','Ali2005','T7!qv#2Lm9$z',2,'Long passwords that mix uppercase, lowercase, numbers and symbols, and are not real words, are hardest to guess.'),(49,7,'How should a website store passwords?','As plain text','As a hash that cannot be turned back into the password','In the browser',1,'A hash is a one-way scramble. The website compares hashes at sign-in and never needs the real password.'),(50,7,'What is the difference between authentication and authorization?','Authentication checks who you are and authorization checks what you are allowed to do',' They mean exactly the same thing','Authentication is for admins only',0,'Signing in proves who you are. Your role then decides which pages and actions you can use.'),(51,7,'What is phishing?','A fake message or website that tricks people into giving away their details','A way to make websites faster','A type of strong password',0,'Phishing pretends to be someone you trust, such as a bank, to steal passwords or personal information.'),(52,8,'What does JavaScript add to a web page?','Colour and fonts',' Interaction, such as reacting to clicks and typing','Database tables',1,'HTML is content, CSS is style, and JavaScript makes the page respond to the user.'),(53,8,'Which keyword creates a variable whose value will not change?','let','var','const',2,'const is for values that stay the same. let is for values that will change.'),(54,8,'What type of value is ‘Hello’?','A number','A string','A boolean',1,'Text inside quotes is a string. Numbers have no quotes, and boolean are true or false.'),(55,8,'What does addEventListener do?','Run a function every time the element is clicked','Deletes the element','Changes the page colour ',0,'An event listener waits for an event, such as a click, and runs its function each time it happens.'),(56,8,' In an array, what position number does the first item have?','1','0','-1',1,'Array positions start at 0, so the first item is courses[0].');
/*!40000 ALTER TABLE `questions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quiz_answers`
--

DROP TABLE IF EXISTS `quiz_answers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quiz_answers` (
  `attempt_id` int NOT NULL,
  `question_id` int NOT NULL,
  `selected_option` int NOT NULL,
  `is_correct` tinyint(1) NOT NULL,
  PRIMARY KEY (`attempt_id`,`question_id`),
  KEY `fk_answers_question` (`question_id`),
  CONSTRAINT `fk_answers_attempt` FOREIGN KEY (`attempt_id`) REFERENCES `quiz_attempts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_answers_question` FOREIGN KEY (`question_id`) REFERENCES `questions` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quiz_answers`
--

LOCK TABLES `quiz_answers` WRITE;
/*!40000 ALTER TABLE `quiz_answers` DISABLE KEYS */;
INSERT INTO `quiz_answers` VALUES (6,42,1,1),(6,43,0,0),(6,44,2,1),(6,45,0,1),(6,46,1,1);
/*!40000 ALTER TABLE `quiz_answers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quiz_attempts`
--

DROP TABLE IF EXISTS `quiz_attempts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quiz_attempts` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `course_id` int NOT NULL,
  `score` int NOT NULL,
  `total` int NOT NULL,
  `taken_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`id`),
  KEY `ix_attempts_user_date` (`user_id`,`taken_utc`),
  KEY `fk_attempts_enrolment` (`user_id`,`course_id`),
  CONSTRAINT `fk_attempts_enrolment` FOREIGN KEY (`user_id`, `course_id`) REFERENCES `enrollments` (`user_id`, `course_id`) ON DELETE CASCADE,
  CONSTRAINT `ck_attempts_score` CHECK (((`total` > 0) and (`score` >= 0) and (`score` <= `total`)))
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quiz_attempts`
--

LOCK TABLES `quiz_attempts` WRITE;
/*!40000 ALTER TABLE `quiz_attempts` DISABLE KEYS */;
INSERT INTO `quiz_attempts` VALUES (6,9,6,4,5,'2026-10-26 07:33:49');
/*!40000 ALTER TABLE `quiz_attempts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `full_name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(254) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL,
  `role` varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Member',
  `failed_attempts` int NOT NULL DEFAULT '0',
  `lockout_until` datetime DEFAULT NULL,
  `created_utc` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_email` (`email`),
  CONSTRAINT `ck_users_role` CHECK ((`role` in (_utf8mb4'Member',_utf8mb4'Admin')))
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES ('ongjipet1',8,'ongjipet1@gmail.com','AQAAAAIAAYagAAAAEG4uL0mswbACOIYirOQSH3UIYAg/J/UHYGQCwrcUDZWK/I/hpeoFD8cbRGKfiUvmRw==','Member',3,NULL,'2026-10-12 04:40:48'),('dsadsadas',9,'dllm@gmail.com','AQAAAAIAAYagAAAAEH9D973QeZgsgaH6vxzeDD7n6G91UvpueqGK+YXe1Ru2blVVRpS54PYTjXTAzt2XUg==','Admin',0,NULL,'2026-10-20 06:09:10'),('dbqwdwq',10,'dllm1@gmail.com','AQAAAAIAAYagAAAAELBL6r0k2BMr5uzxH5vkHdNrTCmjgSfFMKkf3NXhc1E3Ut3+0sMENZdwOlZP5+XrmQ==','Member',0,NULL,'2026-10-20 06:43:36'),('sdasda',11,'yoyo@gmail.com','AQAAAAIAAYagAAAAEH9D973QeZgsgaH6vxzeDD7n6G91UvpueqGK+YXe1Ru2blVVRpS54PYTjXTAzt2XUg==','Member',0,NULL,'2026-10-31 08:21:20'),('dsadsa',12,'nmb@gmail.com','AQAAAAIAAYagAAAAEKZTPn2FRQojYbG2bk5wOnfJdXeEVS97pyoMRjL8wDBpFtlB02L9dVlCXK8Hz8lRvQ==','Member',0,NULL,'2026-10-31 08:24:34');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'learnhubdb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-31 22:41:40
