-- =========================================================================
-- SCRIPT DE SEMILLA (SEED DATA): PALABRAS PARA LETTERCLASH
-- =========================================================================

USE LetterClashDB;
GO

-- Desactivar el conteo de filas afectadas para mejorar el rendimiento
SET NOCOUNT ON;
GO

PRINT 'Iniciando inserción de palabras semilla en LetterClashDB...';

-- =========================================================================
-- 1. PALABRAS EN ESPAÑOL (50 palabras)
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ARBOL' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ARBOL', 'Planta perenne de tronco lenoso y elevado que se ramifica a cierta altura.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'COMPUTADORA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('COMPUTADORA', 'Maquina electronica capaz de almacenar y procesar datos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'JUEGO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('JUEGO', 'Actividad recreativa sometida a reglas en la que se busca ganar o divertirse.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ESTUDIANTE' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ESTUDIANTE', 'Persona que cursa estudios en un establecimiento de ensenanza.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PROGRAMACION' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PROGRAMACION', 'Proceso de disenar, codificar, depurar y mantener el codigo de programas.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ASTRONOMIA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ASTRONOMIA', 'Ciencia que estudia la estructura y evolucion de los astros.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'BIBLIOTECA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('BIBLIOTECA', 'Local donde se conservan libros ordenados para la lectura o consulta.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'DINOSAURIO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('DINOSAURIO', 'Reptil fosil del periodo mesozoico, generalmente de gran tamano.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'EDIFICIO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('EDIFICIO', 'Construccion estable para ser habitada o para otros usos humanos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'FERROCARRIL' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('FERROCARRIL', 'Camino provisto de dos carriles de hierro sobre los que ruedan los trenes.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GUITARRA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GUITARRA', 'Instrumento musical de cuerda pulsada con caja de resonancia en forma de ocho.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'HOLOGRAMA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('HOLOGRAMA', 'Placa fotografica que produce imagenes tridimensionales.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'INTERNET' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('INTERNET', 'Red informatica mundial para la comunicacion y transmision de datos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'JIRAFA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('JIRAFA', 'Mamifero herbivoro de cuello muy largo y pelaje manchado.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'KILOMETRO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('KILOMETRO', 'Unidad de longitud equivalente a mil metros.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'LITERATURA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('LITERATURA', 'Arte de la expresion verbal que abarca textos escritos y hablados.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'MARIPOSA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('MARIPOSA', 'Insecto volador de cuatro alas vistosas que pasa por metamorfosis.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'NATURALEZA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('NATURALEZA', 'Conjunto de cosas que existen en el mundo sin intervencion humana.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'OCEANO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('OCEANO', 'Gran extension de agua salada que cubre la superficie terrestre.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PINGUINO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PINGUINO', 'Ave marina no voladora de plumaje blanco y negro, adaptada para bucear.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'QUIMICA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('QUIMICA', 'Ciencia que estudia la composicion y propiedades de la materia.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'RASCACIELOS' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('RASCACIELOS', 'Edificio de gran altura y muchos pisos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'SEMAFORO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('SEMAFORO', 'Dispositivo de senales luminosas que regula el trafico vial.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'TELEFONO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('TELEFONO', 'Dispositivo para transmitir senales acusticas a distancia.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'UNIVERSO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('UNIVERSO', 'Totalidad del espacio, del tiempo y de la materia existente.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'VELOCIDAD' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('VELOCIDAD', 'Magnitud fisica que expresa el espacio recorrido por unidad de tiempo.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'VIENTO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('VIENTO', 'Corriente de aire producida en la atmosfera por causas naturales.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'XILOFONO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('XILOFONO', 'Instrumento musical de percusion formado por laminas resonantes.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'YACIMIENTO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('YACIMIENTO', 'Sitio donde se halla de forma natural una roca, mineral o fosil.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ZOOLOGICO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ZOOLOGICO', 'Parque o recinto donde se mantienen animales para su exhibicion.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ACERTIJO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ACERTIJO', 'Pregunta o frase con sentido figurado que se plantea como pasatiempo.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'BALLENA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('BALLENA', 'Mamifero marino de gran tamano con respiracion pulmonar.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'CASCADA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('CASCADA', 'Caida de agua desde cierta altura debido a un desnivel abrupto del rio.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'DESIERTO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('DESIERTO', 'Territorio arenoso o pedregoso que carece de vegetacion por falta de lluvia.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ELEFANTE' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ELEFANTE', 'Mamifero terrestre gigante, con piel gruesa, trompa y grandes colmillos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'FUEGO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('FUEGO', 'Emision de luz y calor producida por la combustion.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GALAXIA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GALAXIA', 'Agrupacion masiva de estrellas, gas, polvo y materia cosmica.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'HORIZONTE' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('HORIZONTE', 'Linea aparente que parece separar el cielo de la tierra.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ISLA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ISLA', 'Porcion de tierra rodeada de agua por todas partes.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'JARDIN' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('JARDIN', 'Terreno donde se cultivan plantas y flores para el adorno.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'LABERINTO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('LABERINTO', 'Lugar formado por caminos entrecruzados de los que es dificil salir.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'MONTAÑA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('MONTAÑA', 'Elevacion natural de terreno de gran altura y pendiente.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'NUBE' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('NUBE', 'Masa de vapor de agua suspendida en la atmosfera.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ORQUESTA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ORQUESTA', 'Conjunto de musicos que interpretan obras con diversos instrumentos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PLANETA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PLANETA', 'Cuerpo celeste solido que gira alrededor de una estrella.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'RIO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('RIO', 'Corriente de agua continua que va a desembocar en el mar o lago.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'SELVA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('SELVA', 'Terreno extenso y muy poblado de arboles y vegetacion densa.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'TIERRA' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('TIERRA', 'Planeta del sistema solar en el que habitamos.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'VOLCAN' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('VOLCAN', 'Conducto de la corteza por el que emerge lava y gases del interior.', 'ESPAÑOL');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ZAPATO' AND Idioma = 'ESPAÑOL')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ZAPATO', 'Calzado que cubre el pie y tiene una suela resistente.', 'ESPAÑOL');


-- =========================================================================
-- 2. PALABRAS EN INGLÉS (50 palabras)
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'APPLE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('APPLE', 'A round fruit with red, green, or yellow skin and crisp white flesh.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'COMPUTER' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('COMPUTER', 'An electronic device for storing and processing data.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GAME' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GAME', 'An activity that one engages in for amusement or fun, often with rules.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'STUDENT' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('STUDENT', 'A person who is studying at a school, college, or university.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PROGRAMMING' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PROGRAMMING', 'The process of writing computer programs.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ASTRONOMY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ASTRONOMY', 'The science that deals with celestial objects, space, and the universe.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'LIBRARY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('LIBRARY', 'A building or room containing collections of books for reading.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'DINOSAUR' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('DINOSAUR', 'A fossil reptile of the Mesozoic era, often of enormous size.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'BUILDING' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('BUILDING', 'A structure with a roof and walls, such as a house or school.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'RAILWAY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('RAILWAY', 'A track made of steel rails along which trains run.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GUITAR' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GUITAR', 'A stringed musical instrument, played by plucking or strumming.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'HOLOGRAM' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('HOLOGRAM', 'A three-dimensional image formed by laser light interference.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'INTERNET' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('INTERNET', 'A global computer network providing information and communication.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GIRAFFE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GIRAFFE', 'A large African mammal with a very long neck and patterned coat.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'KILOMETER' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('KILOMETER', 'A metric unit of measurement equal to 1,000 meters.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'LITERATURE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('LITERATURE', 'Written works, especially those considered of superior artistic merit.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'BUTTERFLY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('BUTTERFLY', 'An insect with two pairs of large wings covered with colorful scales.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'NATURE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('NATURE', 'The physical world collectively, including plants, animals, and landscapes.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'OCEAN' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('OCEAN', 'A very large expanse of sea covering most of the Earth.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PENGUIN' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PENGUIN', 'A flightless seabird of southern seas, with wings modified as flippers.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'CHEMISTRY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('CHEMISTRY', 'The branch of science dealing with composition and properties of substances.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'SKYSCRAPER' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('SKYSCRAPER', 'A very tall building of many stories.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'TRAFFICLIGHT' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('TRAFFICLIGHT', 'A set of automatically operated colored lights for controlling traffic.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'TELEPHONE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('TELEPHONE', 'A system for transmitting voices over a distance using wire or radio.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'UNIVERSE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('UNIVERSE', 'All existing matter and space considered as a whole.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'VELOCITY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('VELOCITY', 'The speed of something in a given direction.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'WIND' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('WIND', 'The perceptible natural movement of the air.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'XYLOPHONE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('XYLOPHONE', 'A musical instrument played by striking a row of wooden bars.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ARCHAEOLOGY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ARCHAEOLOGY', 'The study of human history and prehistory through excavation.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ZOOLOGICAL' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ZOOLOGICAL', 'Relating to zoology, the scientific study of animals.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'RIDDLE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('RIDDLE', 'A question or statement phrased so as to require ingenuity in answering.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'WHALE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('WHALE', 'A very large marine mammal with a blowhole for breathing.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'WATERFALL' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('WATERFALL', 'A cascade of water falling from a height.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'DESERT' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('DESERT', 'A barren area of landscape where little precipitation occurs.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ELEPHANT' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ELEPHANT', 'A very large herbivorous mammal with a trunk, tusks, and large ears.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'FIRE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('FIRE', 'Combustion or burning, in which substances combine chemically with oxygen.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GALAXY' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GALAXY', 'A system of stars, gas, and dust held together by gravity.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'HORIZON' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('HORIZON', 'The line at which the earths surface and the sky appear to meet.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ISLAND' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ISLAND', 'A piece of land surrounded by water.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'GARDEN' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('GARDEN', 'A piece of ground used for growing flowers, fruit, or vegetables.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'LABYRINTH' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('LABYRINTH', 'A complicated irregular network of passages or paths.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'MOUNTAIN' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('MOUNTAIN', 'A large natural elevation of the earths surface rising abruptly.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'CLOUD' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('CLOUD', 'A visible mass of condensed water vapor floating in the atmosphere.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'ORCHESTRA' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('ORCHESTRA', 'A group of instrumentalists playing together under a conductor.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'PLANETA' AND Idioma = 'INGLÉS')
    -- Note: Ensure word is PLANET in English (user may have written Planeta by typo, let''s use PLANET)
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('PLANET', 'A celestial body moving in an elliptical orbit round a star.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'RIVER' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('RIVER', 'A large natural stream of water flowing in a channel to the sea.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'JUNGLE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('JUNGLE', 'An area of land overgrown with dense forest and tangled vegetation.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'EARTH' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('EARTH', 'The planet on which we live; the world.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'VOLCANO' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('VOLCANO', 'A mountain having a crater through which lava and gas emerge.', 'INGLÉS');

IF NOT EXISTS (SELECT 1 FROM Palabra WHERE Palabra = 'SHOE' AND Idioma = 'INGLÉS')
    INSERT INTO Palabra (Palabra, Descripcion, Idioma) VALUES ('SHOE', 'A covering for the foot, typically made of leather, with a sturdy sole.', 'INGLÉS');


PRINT 'Inserción de palabras semilla finalizada con éxito.';
GO
