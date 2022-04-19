using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace caseSanctionScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetHtmlAsync method starts here
            GetHtmlAsync();
            Console.ReadLine();

        }

        private static async void GetHtmlAsync()
        {
            double totalPrice = 0;
            int itemCount = 0;

            //Takes a product name from user to search on ebay
            Console.WriteLine("Enter product name: ");
            string searchValue = Console.ReadLine();

            //Creates an URL according to the searchValue, then creates a http client and using URL and HttpClient gets html page
            var url = "https://www.ebay.com/sch/i.html?_from=R40&_nkw="+searchValue+"&_in_kw=1&_ex_kw=&_sacat=0&LH_Complete=1&_udlo=&_udhi=&_samilow=&_samihi=&_sadis=15&_stpos=&_sargn=-1%26saslc%3D1&_salic=1&_sop=12&_dmd=1&_ipg=60&_fosrp=1";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            //creates a file named report.txt in current project directory and path is ->caseSanctionScanner->bin->Debug->netcoreapp3.1
            var currentDirectory = Directory.GetCurrentDirectory();
            StreamWriter sw = new StreamWriter(currentDirectory + "/report.txt");
            sw.Write("Product Searching Report");
            sw.WriteLine();
            sw.Flush();

            //creates a useable html document for further operations
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //gets list of products as html. It goes ul elements which has ListViewInner as id value
            var productsHtml = htmlDocument.DocumentNode.Descendants("ul").
                Where(node => node.GetAttributeValue("id", "")
                .Equals("ListViewInner")).ToList();
            

            //gets only items from list. It goes li element and check for if it contains word 'item' in id value.
            var productListItems = productsHtml[0].Descendants("li")
                .Where(node => node.GetAttributeValue("id", "")
               .Contains("item")).ToList();

            //print total item number to console
            Console.WriteLine();
            itemCount = productListItems.Count();
            Console.WriteLine( itemCount + " Items Found");

            //foreach loop gets item one by one from previous list.
            foreach (var productListItem in productListItems)
            {
                //prints to console listingid value of current item
                Console.WriteLine(
                   "Listing ID: " + productListItem.GetAttributeValue("listingid", ""));

                //takes product name from h3 tag if class value equals to lvtitle
                var productName =  productListItem.Descendants("h3")
                    .Where(node => node.GetAttributeValue("class", "")
                   .Equals("lvtitle")).FirstOrDefault().InnerText.Trim('\r', '\n', '\t');
                Console.WriteLine("Name: " + productName);
                //writes product name to repprt.txt
                sw.WriteLine("Name: " + productName);
                sw.Flush();

                //takes product price from li tag if class value equals to lvprice prc. Regex operation for avoid some printing problems
                var productPrice = Regex.Match(
                    productListItem.Descendants("li")
                    .Where(node => node.GetAttributeValue("class", "")
                   .Equals("lvprice prc")).FirstOrDefault().InnerText.Trim('\r', '\n', '\t')
                   , @"\d+.\d+");

                //prints price to console
                Console.WriteLine("Price: " + productPrice);
                //writes price to report.txt
                sw.WriteLine("Price: " + productPrice);
                sw.WriteLine();
                sw.Flush();
                //sums all the prices and saves to totalPrice variable
                totalPrice = totalPrice + double.Parse(productPrice.ToString());

                //takes item link from href value
                Console.WriteLine(
                   "Item Link: " + productListItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "")
                    );

                Console.WriteLine();

                
            }
            sw.WriteLine();
            //calculates avarega price and writes to report.txt
            sw.WriteLine("Avarage Price is " + (totalPrice / itemCount));
            sw.Flush();
            //closes stream
            sw.Close();
            //for keep console open
            Console.ReadLine();
            
            
            
        }
    }
}
