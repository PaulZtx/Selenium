using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WpfApp2
{
    class Selenya
    {
        private string login = "";
        private string password = "";

        private const string classNews = "_post";
        private const string textNews = "wall_post_text";

        public Selenya(string log, string pas)
        {
            login = log;
            password = pas;
        }

        public void Start(object flag )
        {
            if (login != "" && password != "")
            {
               
                var chromeOptions = new ChromeOptions();
                //Настройка, которая заставляет ждать вебдрайвер загрузки всей страницы перед тем, как ввести данные
                chromeOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                chromeOptions.AddArgument("headless");
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
                    string code = @"window.scrollTo(0, document.body.scrollHeight);";

                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    WebDriverWait w = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    w.Until(e => ScrollPage(10, code, js) == true);

                    List<VkNews> news = new List<VkNews>();
                    List<VkNewsThree> n = new List<VkNewsThree>();
                    //GetNewsTypeOne(driver, ref news);
                    // string json = GetJsonNews(news);
                    //CreateFilejson("", "1.json", json);
                    GetNewsTypeThree(driver, ref n);

                }
                catch
                {
                    MainWindow.ShowMessage("Что-то пошло не так!");
                }

                driver.Quit(); // завершение сессии
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
        /// <summary>
        /// Метод возвращает дату от новости в формате дд.мм.гггг час:мин
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static DateTime GetDatatime(string str)
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
                datas[1] = int.Parse(dateSplit[1]); //day
                datas[2] = int.Parse(dateSplit[0]); //month
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
            return new DateTime();
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
                news.Add(new VkNewsTwo() { Id = elem, ImagePath = imagePath });
            }

        }

        static void GetNewsTypeThree(IWebDriver driver, ref List<VkNewsThree> news)
        {
            WebDriverWait waits = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
            IList<IWebElement> results = waits.Until(e => driver.FindElements(By.XPath("//*[@class='feed_row ' or @class='feed_row']/div")));

            if (results.Count == 0)
                return;
            string[] links = null;
            foreach (var element in results)
            {
                string elem = element.GetAttribute("id");
                try
                {
                    string e = elem.Substring(5);
                    IWebElement el = driver.FindElement(By.XPath("//*[@id=\"wpt-" + e + "\"]/div"));
                    IList<IWebElement> lister = el.FindElements(By.TagName("a"));

                    
                    List<string> l = new List<string>();

                    foreach(var ll in lister)
                        l.Add(ll.Text);

                    links = new string[l.Count];

                    for (int i = 0; i < l.Count; ++i)
                        if(l[i] != null)
                            links[i] = l[i];
                }
                catch
                {

                }
                news.Add(new VkNewsThree() { Id = elem, Links = links });
            }
        }

        /// <summary>
        /// Получение json в строке с помощью метода JsonSerializer
        /// </summary>
        /// <param name="news"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static string GetJsonNews<T>(List<T> news)
        {
            string json = "[";

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            foreach (var nw in news)
            {
                // Console.WriteLine(nw.Id + " " + nw.Text);
                json += JsonSerializer.Serialize<T>(nw, options) + ", \n";
            }

            json += "]";

            return json;
        }

        static void CreateFilejson(string path, string fileName, string json)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path + fileName, false))
                {
                    for (int i = 0; i < json.Length; ++i)
                        sw.Write(json[i]);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка при записи в файл {fileName}\nТип ошибки: {exception.Message}");
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
