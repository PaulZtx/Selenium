using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace WpfApp2
{
    class Selenya
    {
        public static bool IsWork;
        public delegate void EndWork();
        public event EndWork endwork;
        private string login = "";
        private string password = "";

        private const string classNews = "_post";
        private const string textNews = "wall_post_text";
        private List<VkNews> news1;
        private List<VkNewsTwo> news2;
        private List<VkNewsThree> news3;


        public delegate void ChangeLabel(string mes);
        public event ChangeLabel changer;

        public Selenya(string log, string pas)
        {
            login = log;
            password = pas;
        }

        public void Start(object flag)
        {
            if (login != "" && password != "")
            {

                var chromeOptions = new ChromeOptions();
                //Настройка, которая заставляет ждать вебдрайвер загрузки всей страницы перед тем, как ввести данные
                chromeOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                //chromeOptions.AddArgument("headless");
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;


                IWebDriver driver = new ChromeDriver(service, chromeOptions);
                driver.Navigate().GoToUrl(@"https://vk.com/");

                IWebElement passwordElement = driver.FindElement(By.Id("index_pass"));
                //поиск тега, который находится выше ввода пароля
                IWebElement loginElement = driver.FindElement(
                    RelativeBy.WithTagName("input").Above(passwordElement));

                loginElement.SendKeys(login);
                passwordElement.SendKeys(password);

                //Ищем кнопку войти
                driver.FindElement(By.Id("index_login_button")).Click();

                try
                {
                    //Явное ожидание до получения заголовка "Новости"
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                    wait.Until(e => driver.Title == "Новости");
                    MainWindow.ShowMessage("Вход выполнен!");

                    //string code = @"window.scrollTo(0, document.body.scrollHeight);";

                    //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    // WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    //w.Until(e => ScrollPage(10, code, js) == true);
                    //Thread.Sleep(100);

                    news1 = new List<VkNews>();
                    news2 = new List<VkNewsTwo>();
                    news3 = new List<VkNewsThree>();


                    while (IsWork)
                    {
                        GetNews2(driver, ref news1, ref news2, ref news3);

                        Thread thread1 = new Thread(() => CreateFilejson("1.json"));
                        thread1.Name = "Thread1";

                        Thread thread2 = new Thread(() => CreateFilejson("2.json"));
                        thread2.Name = "Thread2";

                        Thread thread3 = new Thread(() => CreateFilejson("3.json"));
                        thread3.Name = "Thread3";


                        thread1.Start();
                        thread2.Start();
                        thread3.Start();


                        Thread.Sleep(12000);
                        driver.Navigate().Refresh();
                    }

                }
                catch (Exception mes)
                {
                    MainWindow.ShowMessage("Что-то пошло не так!\n" + mes);
                }

                driver.Quit(); // завершение сессии
            }

            endwork?.Invoke();
        }

        static void GetNews2(IWebDriver driver, ref List<VkNews> news1, ref List<VkNewsTwo> news2, ref List<VkNewsThree> news3)
        {
            WebDriverWait waits = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement element = waits.Until(e => driver.FindElement(By.XPath(@"//*[@id='feed_rows']//*")));

            //IWebElement element = driver.FindElement(By.XPath(@"//*[@id='feed_rows']//*"));
            //IWebElement text = element.FindElement(By.XPath(@"//*[@id='feed_rows']//*[@class='page_post_sized_thumbs  clear_fix']/a"));

            IWebElement div = element.FindElement(By.ClassName("_post"));
            string id = div.GetAttribute("id");

            IList<IWebElement> imgs;
            string[] links_img = null;

            string txt = "";


            try
            {
                IWebElement text = element.FindElement(By.ClassName("wall_post_text"));
                txt = text.Text;
                if (txt.IndexOf("#") != -1)
                {
                    string temp = "";
                    string[] arr = txt.Split(" ");
                    for (int i = 0; i < arr.Length; ++i)
                    {
                        if (arr[i].IndexOf("#") != -1)
                        {
                            continue;
                        }
                        temp += arr[i];
                    }
                    txt = temp == "" ? "Null" : temp;
                }
            }
            catch
            {
                txt = "Нет текста у новости";
            }



            try
            {
                imgs = element.FindElement(By.XPath(@"//*[@id='feed_rows']//*[@class='page_post_sized_thumbs  clear_fix']"))
                    .FindElements(By.TagName("a"));
                links_img = new string[imgs.Count];
                int i = 0;

                foreach (var img in imgs)
                {
                    string path = img.GetAttribute("style");
                    string imagePath = path.Substring(path.IndexOf("url") + 5);
                    imagePath = imagePath.Remove(imagePath.Length - 3);
                    links_img[i++] = imagePath;

                    //Console.WriteLine(imagePath);
                }
            }
            catch
            {
                Console.WriteLine("нет картинок");
            }


            //Переделать
            string link = "";
            try
            {
                IWebElement text = element.FindElement(By.ClassName("wall_post_text"));
                IList<IWebElement> links = text.FindElements(By.TagName("a"));

                foreach (var l in links)
                {
                    link += l.Text + "\n";
                }

            }
            catch
            {

            }

            IWebElement el;
            try
            {
                el = driver.FindElement(By.XPath("//*[@id=\"" + id + "\"]//*[@class=\"post_link\"]/span"));
            }
            catch
            {
                el = null;
            }


            DateTime data;
            if (el != null)
            {
                if (el.GetAttribute("time") != null)
                    data = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(
                        Convert.ToDouble(el.GetAttribute("time")));
                else
                    data = GetDatatime(el.Text);
                //Console.WriteLine(el.Text);
                //Thread.Sleep(5000);
            }
            else data = new DateTime();

            news1.Add(new VkNews { Id = id, Text = txt, Time = data });
            news2.Add(new VkNewsTwo { Id = id, ImagePath = links_img });
            news3.Add(new VkNewsThree { Id = id, Links = link });

            //MainWindow.ShowMessage($"id = {id}, text = {txt}, data = {data}, links = {link}");
            //Console.WriteLine($"id = {id}, text = {txt}, data = {data}, links = {link}");
        }


        /// <summary>
        /// Метод возвращает дату от новости в формате дд.мм.гггг час:мин
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static DateTime GetDatatime(string str)
        {
            try
            {
                string[] words = str.Split(' ');
                string data = "";
                int[] datas = new int[5];

                if (words.Length == 3)
                {
                    string date;

                    if (words[0] == "сегодня")
                        date = DateTime.Now.ToString();

                    else
                        date = DateTime.Now.AddDays(-1).ToString();

                    string[] dateSplit = date.Split('.');
                    datas[0] = int.Parse(dateSplit[2].Split(' ')[0]); //year
                    datas[1] = int.Parse(dateSplit[0]); //day
                    datas[2] = int.Parse(dateSplit[1]); //month
                    datas[3] = int.Parse(words[2].Split(':')[0]); // hour
                    datas[4] = int.Parse(words[2].Split(':')[1]); //minutes

                    return new DateTime(datas[0], datas[2], datas[1], datas[3], datas[4], 0);

                }

                if (words.Length == 4)
                {
                    string[] months = { "янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек" };
                    data += DateTime.Parse(DateTime.Today.ToString()).Year.ToString() + ".";
                    for (int i = 0; i < 12; ++i)
                    {
                        if (months[i] == words[1]) data += (i + 1).ToString() + ".";
                    }

                    data += words[0] + ".";
                    string[] date = words[3].Split(":");
                    data += date[0] + "." + date[1];
                    string[] newDate = data.Split(".");

                    return new DateTime(int.Parse(newDate[0]), int.Parse(newDate[1]), int.Parse(newDate[2]),
                        int.Parse(newDate[3]), int.Parse(newDate[4]), 0);
                }
            }
            catch
            {

            }
            return new DateTime();
        }
        /// <summary>
        /// Получение json в строке с помощью метода JsonSerializer
        /// </summary>
        /// <param name="news"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static string GetJsonNews<T>(T news)
        {
            string json = JsonConvert.SerializeObject(news);
            return json;
        }

        static object locker = new object();

        /// <summary>
        /// Создание файла json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="json"></param>
        private void CreateFilejson(string fileName) //лучше переделать
        {
            lock (locker)
            {


                try
                {


                    if (ReadJson(fileName) == null)
                    {
                        string wr = "[";
                        switch (int.Parse(fileName[0].ToString()))
                        {
                            case 1:
                                wr += GetJsonNews(news1.Last());
                                break;
                            case 2:
                                wr += GetJsonNews(news2.Last());
                                break;
                            case 3:
                                wr += GetJsonNews(news3.Last());
                                break;

                        }
                        wr += "\n]";
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            sw.Write(wr);
                        }
                        MainWindow.ShowMessage(wr);
                    }

                    else
                    {
                        string file = ReadJson(fileName);
                        string wr = "";

                        switch (int.Parse(fileName[0].ToString()))
                        {
                            case 1:

                                if (file.IndexOf(news1.Last().Id) != -1)
                                    return;

                                wr = GetJsonNews(news1.Last());
                                break;
                            case 2:

                                if (file.IndexOf(news2.Last().Id) != -1)
                                    return;

                                wr = GetJsonNews(news2.Last());
                                break;
                            case 3:
                                if (file.IndexOf(news3.Last().Id) != -1)
                                    return;

                                wr = GetJsonNews(news3.Last());
                                break;

                        }

                        int pos = file.IndexOf("]", file.Length - 1);

                        file = file.Remove(pos);
                        file += ",\n";
                        file += wr + "\n]";

                        File.WriteAllText(fileName, String.Empty);
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            sw.Write(file);
                        }
                      
                    }
                }
                catch (Exception exception)
                {
                    MainWindow.ShowMessage("Ошибка: " + exception.ToString());
                }
            }
        }


        static object locker1 = new object();
        /// <summary>
        /// Проверка существования новости по id
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private void IsCreate<T>(string filename, ref List<T> list)
        {
            lock (locker1)
            {
                try
                {
                    string file = "";

                    if (ReadJson(filename) == null)
                        return;
                    else
                        file = ReadJson(filename);

                    IList<T> l = null;

                    if (list.GetType() == typeof(List<VkNews>))
                        l = (IList<T>)JsonConvert.DeserializeObject<List<VkNews>>(file);


                    else if (list.GetType() == typeof(List<VkNewsTwo>))
                        l = (IList<T>)JsonConvert.DeserializeObject<List<VkNewsTwo>>(file);

                    else if (list.GetType() == typeof(List<VkNewsThree>))
                        l = (IList<T>)JsonConvert.DeserializeObject<List<VkNewsThree>>(file);

                    l = l.Distinct().ToList();
                    foreach (var item in list)
                    {
                        if (l.Contains(item))
                            list.Remove(item);
                    }

                }
                catch (Exception message)
                {
                    MainWindow.ShowMessage("Ошибка: " + message.ToString());
                }

            }
        }


        static object locker2 = new object();

        private string ReadJson(object filename)
        {
            lock (locker2)
            {
                if (!(File.Exists(filename.ToString())))
                    return null;

                string file = File.ReadAllText(filename.ToString());
                //changer?.Invoke("Поток " + Thread.CurrentThread.Name + " прочитал файл.");
                return file;
            }
        }

        /// <summary>
        /// Заполняет переданный по ссылке список новостями первого типа
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="news"></param>
        static void GetNewsTypeOne(IWebDriver driver, ref List<VkNews> news)
        {
            WebDriverWait waits = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
            IList<IWebElement> results = waits.Until(e => driver.FindElements(By.XPath("//*[@class='feed_row ' or @class='feed_row']/div")));

            if (results.Count == 0)
                return;
            string textnews = "";

            foreach (var element in results)
            {
                string elem = element.GetAttribute("id");
                //Console.WriteLine(elem);

                try
                {
                    string e = elem.Substring(5);
                    IWebElement el =
                        driver.FindElement(By.XPath("//*[@id=\"wpt-" + e + "\"]/*[@class=\"wall_post_text\"]"));
                    textnews = el.Text;
                }
                catch
                {
                    textnews = "Нет текста у новости.";
                }
                finally
                {

                    IWebElement el;
                    try
                    {
                        el = driver.FindElement(By.XPath("//*[@id=\"" + elem + "\"]//*[@class=\"post_link\"]/span"));
                    }
                    catch
                    {
                        el = null;
                    }

                    DateTime data;
                    if (el != null)
                    {
                        if (el.GetAttribute("time") != null)
                            data = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(
                                Convert.ToDouble(el.GetAttribute("time")));
                        else
                            data = GetDatatime(el.Text);
                        //Console.WriteLine(el.Text);
                    }
                    else data = new DateTime();

                    news.Add(new VkNews() { Id = elem, Text = textnews, Time = data });
                }
            }
        }
        static void GetNewsTypeTwo(IWebDriver driver, ref List<VkNewsTwo> news)
        {
            WebDriverWait waits = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
            IList<IWebElement> results = waits.Until(e => driver.FindElements(By.XPath("//*[@class='feed_row ' or @class='feed_row']/div")));

            if (results.Count == 0)
                return;

            // string searchImage = "//*[@id="wpt-91895023_1345991"]//a";
            string imagePath = "";

            foreach (var element in results)
            {
                string elem = element.GetAttribute("id");
                //Console.WriteLine(elem);

                try
                {
                    string e = elem.Substring(5);
                    IWebElement el = driver.FindElement(By.XPath("//*[@id=\"wpt-" + e + "\"]//a"));
                    string ipath = el.GetAttribute("style");
                    imagePath = ipath.Substring(ipath.IndexOf("url") + 5);
                    imagePath = imagePath.Remove(imagePath.Length - 3);
                    //Console.WriteLine(imagePath);
                }
                catch
                {
                    imagePath = "Null";
                }
                //news.Add(new VkNewsTwo() { Id = elem, ImagePath = imagePath });
            }

        }

        static void GetNewsTypeThree(IWebDriver driver, ref List<VkNewsThree> news)
        {
            WebDriverWait waits = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
            IList<IWebElement> results = waits.Until(e => driver.FindElements(By.XPath("//*[@class='feed_row ' or @class='feed_row']/div")));

            if (results.Count == 0)
                return;
            string links = "";
            foreach (var element in results)
            {
                string elem = element.GetAttribute("id");
                try
                {
                    links = "";
                    string e = elem.Substring(5);
                    IWebElement el = driver.FindElement(By.XPath("//*[@id=\"wpt-" + e + "\"]/div"));
                    IList<IWebElement> lister = el.FindElements(By.TagName("a"));


                    List<string> l = new List<string>();

                    foreach (var ll in lister)
                        l.Add(ll.Text);

                    for (int i = 0; i < l.Count; ++i)
                        if (l[i] != null)
                            links += l[i] + "\n";
                }
                catch (Exception mes)
                {
                    MainWindow.ShowMessage(mes.ToString());
                }
                news.Add(new VkNewsThree() { Id = elem, Links = links });
            }
        }



        private static bool ScrollPage(int n, string code, IJavaScriptExecutor js)
        {
            for (int i = 0; i < n; ++i)
                js.ExecuteScript(code);
            return true;
        }

    }
}
