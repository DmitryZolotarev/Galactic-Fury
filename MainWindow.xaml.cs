using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows;
using System.Media;
using System.IO;
using System;

namespace WpfApp1 {
    class Entity { public float X, Y, HP; }
    public partial class MainWindow : Window {
        Entity player;       
        float[] param = new float[7];        
        BitmapImage[] img = new BitmapImage[10];       
        DispatcherTimer timer = new DispatcherTimer();
        SoundPlayer music = new SoundPlayer("music.wav");
        bool leftDown, rightDown, spaceDown, F6Down, F7Down;
        float latency1, latency2, SectorID = 0, Damage = 1, W, H, E;
        string Date = "", Header = "Galactic Fury", S = new string(' ', 26);
        HashSet<Entity> enemies = new HashSet<Entity>(),
        playerShots = new HashSet<Entity>(),
        enemyShots = new HashSet<Entity>(),
        HPbonuses = new HashSet<Entity>(),
        PowerUPs = new HashSet<Entity>();
        public MainWindow() {
            music.Load();
            music.PlayLooping();
            InitializeComponent();
            var size = new Form1();
            size.Show(); size.Close();
            W = size.Width; H = size.Height;
            var xSettings = XElement.Load("Параметры.xml");
            param[0] = (float)xSettings.Element("Игрок").Attribute("HP");
            param[1] = (float)xSettings.Element("Игрок").Attribute("скорость");          
            param[2] = (float)xSettings.Element("Враги").Attribute("HP");
            param[3] = (float)xSettings.Element("Враги").Attribute("скорость_снарядов");
            param[4] = (float)xSettings.Element("Враги").Attribute("задержка_огня");
            param[5] = (float)xSettings.Element("HPbonus").Attribute("множитель_лечения");
            param[6] = (float)xSettings.Element("PowerUP").Attribute("новый_урон");
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.Tick += OnTimer;
            var main_menu = new Window1();         
            main_menu.Show(); m:
            var new_game = MessageBox.Show
            (S + "Начать новую игру?", Header, MessageBoxButton.YesNoCancel);          
            if (new_game == MessageBoxResult.Yes) {            
                StartLevel();
                Level_ID();
            }
            else if (new_game == MessageBoxResult.No) {
                new_game = MessageBox.Show
                (new string(' ', 17)+"Загрузить сохранённую игру?"+
                new string(' ', 12), Header, MessageBoxButton.YesNo);
                if (new_game == MessageBoxResult.Yes) {
                    try {                     
                        StartLevel(); Load();
                    } catch (FileNotFoundException) {
                        MessageBox.Show(new string(' ', 18)+"Файл сохранения не найден!"
                        + new string(' ', 13), Header); goto m;
                    }
                    Level_ID();
                }
                else goto m;
            }
            else Application.Current.Shutdown();
            main_menu.Close();          
            timer.Start();         
        }       
        void StartLevel() {     
            Stats.Margin = new Thickness(W - 340, -10, 0, 0);
            for (int I = 0; I < 10; I++) img[I] = new BitmapImage
            (new Uri($"Sprites\\{I}.png", UriKind.Relative));          
            Stats.Foreground = Brushes.Red;
            Background = Brushes.Transparent;
            enemies.Clear(); E = 0;
            player = new Entity() 
            { Y = H - 100, X = (W / 2) - 40, HP = param[0] };
            for (float I = 1; I <= 5; I++)
            for (float J = 0; J < W / 125; J++) {
                var enemy = new Entity() {
                    X = J * 133,
                    Y = I * 50,
                    HP = param[2]
                }; E++;
                enemies.Add(enemy);               
            }
        }
        private void Level_ID() {
            if (Date == "" || SectorID == 0) {
                float day = new Random().Next(1, 29),
                month = new Random().Next(1, 13),
                year = new Random().Next(3000, 3201);
                Date = $"{day}.{month}.{year}";
                SectorID = new Random().Next(1000, 1000001);              
            }
            var S = $"        {Date},  Битва в секторе № {SectorID}     ";
            MessageBox.Show(S, Header);
        }
        void Load() {
            enemies.Clear();
            PowerUPs.Clear();
            HPbonuses.Clear();
            enemyShots.Clear();
            playerShots.Clear();
            var xSave = XElement.Load("Save.xml");
            latency2 = (float)xSave.Attribute("EnemyLatency");
            SectorID = (float)xSave.Attribute("SectorID");
            E = (float)xSave.Attribute("EnemiesWere");
            Date = (string)xSave.Attribute("Date");           

            Damage = (float)xSave.Element("Player").Attribute("Damage");
            player.X = (float)xSave.Element("Player").Attribute("X");
            player.Y = (float)xSave.Element("Player").Attribute("Y");
            player.HP = (float)xSave.Element("Player").Attribute("HP");
            
            foreach (var xEnemy in xSave.Element("SavedEnemies").Elements()) {
                var enemy = new Entity() {
                    X = (float)xEnemy.Attribute("X"),
                    Y = (float)xEnemy.Attribute("Y"),
                    HP = (float)xEnemy.Attribute("HP")
                };
                enemies.Add(enemy);
            }
            foreach (var xEnemyShot in xSave.Element("EnemyShots").Elements()) {
                var enemyShot = new Entity() {
                    X = (float)xEnemyShot.Attribute("X"),
                    Y = (float)xEnemyShot.Attribute("Y"),
                };
                enemyShots.Add(enemyShot);
            }
            foreach (var xPlayerShot in xSave.Element("PlayerShots").Elements()) {
                var playerShot = new Entity() {
                    X = (float)xPlayerShot.Attribute("X"),
                    Y = (float)xPlayerShot.Attribute("Y"),
                };
                playerShots.Add(playerShot);
            }
            foreach (var xHPbonus in xSave.Element("HPbonuses").Elements()) {
                var HPbonus = new Entity() {
                    X = (float)xHPbonus.Attribute("X"),
                    Y = (float)xHPbonus.Attribute("Y"),
                };
                HPbonuses.Add(HPbonus);
            }
            foreach (var xPowerUP in xSave.Element("PowerUPs").Elements()) {
                var PowerUP = new Entity() {
                    X = (float)xPowerUP.Attribute("X"),
                    Y = (float)xPowerUP.Attribute("Y"),
                };
                PowerUPs.Add(PowerUP);
            }
            F7Down = false; 
        }
        void OnTimer(object sender, EventArgs e) {
            var Deleted_Entities = new HashSet<Entity>();
            if (Damage == param[6]) img[1] = img[8];
            latency2++;
            foreach (var enemy in enemies) {
                enemy.X++;
                if (enemy.X > W) {
                    enemy.Y += 50;
                    enemy.X = -100;
                }
                if (latency2 % param[4] == 0) {
                    var enemyShot = new Entity {
                        X = (float)(enemy.X + img[0].Width / 2),
                        Y = (float)(enemy.Y + img[0].Height)
                    };
                    enemyShots.Add(enemyShot);
                }
                foreach (var playerShot in playerShots) {
                    if (playerShot.X + img[1].Width > enemy.X)
                        if (playerShot.X < enemy.X + img[0].Width)
                            if (playerShot.Y < enemy.Y + 25) {
                                Deleted_Entities.Add(playerShot);
                                enemy.HP -= Damage;
                            }
                }
                if (enemy.HP <= 0) {
                    Deleted_Entities.Add(enemy);
                    float chance = new Random().Next(20);
                    if (chance == 0 && Damage == 1) {
                        var PowerUP = new Entity() {
                            X = (float)(enemy.X + img[0].Width / 3),
                            Y = (float)(enemy.Y + img[0].Height)
                        };
                        PowerUPs.Add(PowerUP);
                    }
                    if (chance == 1 || chance == 2) {
                        var HPbonus = new Entity() {
                            X = (float)(enemy.X + img[0].Width / 3),
                            Y = (float)(enemy.Y + img[0].Height)
                        };
                        HPbonuses.Add(HPbonus);
                    }
                }         
                if (enemy.X + img[0].Width >= player.X)
                if (enemy.Y + 50 >= player.Y) player.HP = 0;
            }
            foreach  (var HPbonus in HPbonuses) {
                if (HPbonus.Y > H) Deleted_Entities.Add(HPbonus);
                else if (HPbonus.X >= player.X && HPbonus.Y > player.Y)
                if (HPbonus.X - img[4].Width <= player.X + img[4].Width) {
                    player.HP += param[5];
                    if (player.HP > 100) player.HP = 100;
                    Deleted_Entities.Add(HPbonus);
                }
                HPbonus.Y += 5;
            }
            foreach (var PowerUP in PowerUPs) {
                if (PowerUP.Y > H) Deleted_Entities.Add(PowerUP);
                else if (PowerUP.X >= player.X && PowerUP.Y > player.Y)
                    if (PowerUP.X - img[5].Width <= player.X + img[5].Width) {
                        Deleted_Entities.Add(PowerUP);                       
                        Damage = param[6];
                    }
                PowerUP.Y += 5;
            }
            foreach (var enemyShot in enemyShots) {
                if (enemyShot.X >= player.X && enemyShot.Y > player.Y)
                if (enemyShot.X + 9 <= player.X + img[3].Width) {
                    Deleted_Entities.Add(enemyShot);
                    player.HP--;
                }
                enemyShot.Y += param[3];
                if (enemyShot.Y > H) Deleted_Entities.Add(enemyShot);
            }
            foreach (var playerShot in playerShots) {
                playerShot.Y -= 25;
                if (playerShot.Y < 0) Deleted_Entities.Add(playerShot);
            }
            foreach (var entity in Deleted_Entities) {
                playerShots.Remove(entity);
                enemyShots.Remove(entity);
                enemies.Remove(entity);
                HPbonuses.Remove(entity);
                PowerUPs.Remove(entity);
            }
            if (leftDown && !rightDown && player.X > 5) {
                img[3] = img[6];
                player.X -= param[1];
            }
            else if (rightDown && !leftDown && player.X <= W - 105) {
                img[3] = img[7];
                player.X += param[1];
            }
            else img[3] = new BitmapImage(new Uri("Sprites\\3.png", UriKind.Relative));
            if (spaceDown) {                
                latency1++;
                if (latency1 % 4 == 0) {
                    var playerShot = new Entity() {
                        X = player.X + (float)(img[3].Width / 2 - img[1].Width / 2),
                        Y = player.Y - 20
                    };
                    playerShots.Add(playerShot);
                }
            }
            if (F6Down) {
                var xSave = new XElement("Save");
                xSave.Add(new XAttribute("EnemiesWere", E));
                xSave.Add(new XAttribute("Date", Date));
                xSave.Add(new XAttribute("SectorID", SectorID));
                xSave.Add(new XAttribute("EnemyLatency", latency2 % param[4]));

                var xPlayer = new XElement("Player");
                xPlayer.Add(new XAttribute("X", player.X));
                xPlayer.Add(new XAttribute("Y", player.Y));               
                xPlayer.Add(new XAttribute("HP", player.HP));
                xPlayer.Add(new XAttribute("Damage", Damage));
                xSave.Add(xPlayer);

                var xEnemies = new XElement("SavedEnemies");
                var xEnemyShots = new XElement("EnemyShots");
                var xPlayerShots = new XElement("PlayerShots");
                var xHPbonuses = new XElement("HPbonuses");
                var xPowerUPs = new XElement("PowerUPs");

                foreach (var enemy in enemies) {
                    var xEnemy = new XElement("Enemy");
                    xEnemy.Add(new XAttribute("X", enemy.X));
                    xEnemy.Add(new XAttribute("Y", enemy.Y));
                    xEnemy.Add(new XAttribute("HP", enemy.HP));
                    xEnemies.Add(xEnemy);
                }
                foreach (var playerShot in playerShots) {
                    var xPlayerShot = new XElement("PlayerShot");
                    xPlayerShot.Add(new XAttribute("X", playerShot.X));
                    xPlayerShot.Add(new XAttribute("Y", playerShot.Y));
                    xPlayerShots.Add(xPlayerShot);
                }
                foreach (var enemyShot in enemyShots) {
                    var xEnemyShot = new XElement("EnemyShot");
                    xEnemyShot.Add(new XAttribute("X", enemyShot.X));
                    xEnemyShot.Add(new XAttribute("Y", enemyShot.Y));
                    xEnemyShots.Add(xEnemyShot);
                }
                foreach (var HPbonus in HPbonuses) {
                    var xHPbonus = new XElement("HPbonus");
                    xHPbonus.Add(new XAttribute("X", HPbonus.X));
                    xHPbonus.Add(new XAttribute("Y", HPbonus.Y));
                    xHPbonuses.Add(xHPbonus);
                }
                foreach (var PowerUP in PowerUPs) {
                    var xPowerUP = new XElement("PowerUP");
                    xPowerUP.Add(new XAttribute("X", PowerUP.X));
                    xPowerUP.Add(new XAttribute("Y", PowerUP.Y));
                    xPowerUPs.Add(xPowerUP);
                }
                xSave.Add(xEnemies);
                xSave.Add(xEnemyShots);
                xSave.Add(xPlayerShots);
                xSave.Add(xHPbonuses);
                xSave.Add(xPowerUPs);

                File.WriteAllText("Save.xml", $"{xSave}");
                F6Down = false;
            }
            if (F7Down) {
                try {
                    SectorID = 0;
                    Date = ""; Load(); 
                                                                             
                } catch (FileNotFoundException) { }
            }         
            if (player.HP <= 0 || enemies.Count == 0) {                
                Date = "";
                SectorID = 0;
                leftDown = false;
                rightDown = false;
                spaceDown = false;
                float score = E - enemies.Count;
                playerShots.Clear();
                enemyShots.Clear();
                HPbonuses.Clear();
                PowerUPs.Clear();
                enemies.Clear();
                InvalidateVisual();
                Stats.Foreground = Brushes.Transparent;
                if (player.HP > 0) MessageBox.Show(S + "   Победа!" + S, Header);
                else MessageBox.Show(S + "   Вас убили!" + S, "Galactic Fury HD");
                MessageBox.Show(S + $"   Ваш счёт: {score}" + S, Header);
                latency1 = 0; latency2 = 0;             
                Level_ID();
                StartLevel();
                Damage = 1;
            }            
            InvalidateVisual();
        }
        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.Left) leftDown = true;
            if (e.Key == Key.Right) rightDown = true;
            if (e.Key == Key.Space) spaceDown = true;
            if (e.Key == Key.Escape) {
                timer.Stop();
                leftDown = false; rightDown = false; spaceDown = false;
                var text = S + "Выйти в главное меню?" + new string(' ', 21);
                var pause = MessageBox.Show(text, "Пауза", MessageBoxButton.OKCancel);
                if (pause == MessageBoxResult.OK) {
                    var new_game = new MainWindow();
                    new_game.Show(); Close();
                }
                else timer.Start();
            }
            if (e.Key == Key.F6) F6Down = true;
            if (e.Key == Key.F7) F7Down = true;
        }
        protected override void OnKeyUp(KeyEventArgs e) {
            if (e.Key == Key.Left) leftDown = false;
            if (e.Key == Key.Right) rightDown = false;
            if (e.Key == Key.Space) spaceDown = false;
        }
        protected override void OnRender(DrawingContext draw) {
            draw.DrawImage(img[9], new Rect (0, 0, W, H));
            
            foreach (var enemy in enemies) Render(enemy, img[0]);
            foreach (var playerShot in playerShots) Render(playerShot, img[1]);
            foreach (var enemyShot in enemyShots) Render(enemyShot, img[2]);
            foreach (var HPbonus in HPbonuses) Render(HPbonus, img[4]);
            foreach (var PowerUP in PowerUPs) Render(PowerUP, img[5]);
            if (player.HP > 0 || enemies.Count > 0) Render(player, img[3]);
            
            Stats.Content = $"Осталось врагов: {enemies.Count}     ♥ {player.HP}";
            void Render(Entity E, BitmapImage img) =>
            draw.DrawImage(img, new Rect(E.X, E.Y, img.Width, img.Height));          
        }
    }
}