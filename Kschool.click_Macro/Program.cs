using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kschool.click_Macro
{
    internal static class Program
    {
        static string user_agent = "";
        static string school_name = "";
        static string school_id = "";
        static string token = "";

        static void Main()
        {
            //
            Console.Title = "KSchool.Click";
            Console.WriteLine("Kschool.Click");
            Thread.Sleep(3000);
            Console.Beep();
            Console.Clear();
            Console.Write("학교 ID: ");
            school_id = Console.ReadLine();
            Console.Write("학교 이름: ");
            school_name = Console.ReadLine();
            Console.Write("User-agent(기본값 1): ");
            string read_useragent = Console.ReadLine();
            user_agent = ((read_useragent == "1") ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36" : read_useragent);
            Console.Beep();
            Console.Clear();


            gettoken();


            int i = 0;
            int rei = 0;
            while (true)
            {
                try
                {
                    post();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);//토큰 만료 처리
                    try { ++rei; gettoken(); } catch (Exception e2) { Console.WriteLine(e2); }

                }
                Console.Title = $"보낸 횟수: {i} | 총 클릭수: {i * 200} | get_token : {rei}";
                Thread.Sleep(15000); //원래 20초인데 최대 15초 까지 가능한듯 
            }




            Console.ReadKey();
        }

        static void post()
        {
            //리퀘스트 보내기 ㅣㅢ ㅋㅋ
            WebRequest request = (HttpWebRequest)WebRequest.Create($"https://port-0-kschool2-backend-4i0mp24lct3difg.jocoding.cloud/pop?schoolCode=7692183&count={200}&token={token}");
            request.Method = "POST";
            string postData = "{schoolCode : '" + school_id + "', count : 200, token : '" + token + "'}";
            //richTextBox1.Text = postData;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json";
            ((System.Net.HttpWebRequest)request).UserAgent = user_agent;
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(postData.ToString());
                Console.WriteLine(responseFromServer.ToString());
                token = responseFromServer.ToString().Split('/')[3]; //token 얻기
            }
        }

        static string gettoken()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddExtension(System.IO.Directory.GetCurrentDirectory() + @"\chrome.zip"); //캡챠 Solver API 
                                                                                              //학교 대항전 끝나면 공개 생각해봄 ㅅㄱ
            options.AddArgument($"--user-agent={user_agent}"); //User-agent ex:(Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36)
            //options.AddArgument("--headless"); //창 숨기는거
            
            //영어 자잘자잘 뜨는거 끄기
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;


            using (IWebDriver driver = new ChromeDriver(options: options, service: driverService))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(200); //대기 시간

                //학교 검색 후 들어가기
                driver.Url = "https://kschool.click/findSchool";
                driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/span[2]")).Click();
                driver.FindElement(By.XPath("/html/body/div/div[1]/div[2]/input")).SendKeys(school_name);
                driver.FindElement(By.XPath("/html/body/div/div[1]/div[2]/button")).Click();
                driver.FindElement(By.XPath("/html/body/div/div[1]/div[3]/div/div[1]")).Click();


            retry:
                string retoken = "";
                try
                { 
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript(@"let x = document.getElementsByClassName('pop_title__XnUNI')[0]; x.innerText = window.token;");
                    string title = driver.FindElement(By.XPath("/html/body/div/div[2]/div[3]")).Text;
                    if (title == "undefined" || title == "")
                    {
                        //대기...
                        Thread.Sleep(5 * 1000);
                        goto retry;
                    }
                    retoken = title;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Thread.Sleep(5000);
                    goto retry;
                }
                Console.WriteLine("token: " + retoken);

                return retoken;
            }
        }
    }
}
