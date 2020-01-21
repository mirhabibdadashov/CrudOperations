using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CRUDProject.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CRUDProject.Controllers
{
    public class HomeController : Controller
    {
        private CrudDbContext _db;
        public HomeController(CrudDbContext Db)
        {
            _db = Db;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            List<Product> products = await _db.Products
                                                    .Skip(5 * (page - 1))
                                                        .Take(5)
                                                            .ToListAsync();
            foreach (Product product in products)
            {
                var client = new RestClient("https://google-translate1.p.rapidapi.com/language/translate/v2");
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-rapidapi-host", "google-translate1.p.rapidapi.com");
                request.AddHeader("x-rapidapi-key", "e219260d74mshc9842dc6a717bd6p187982jsnddeece884433");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", $"source=az&q={product.Description}&target=en", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(response.Content);
                    JToken data = json["data"];
                    JToken translations = data["translations"];
                    JToken firstTranslations = translations.First;
                    product.DescEng = firstTranslations["translatedText"].ToString();
                }
                else
                {
                    product.DescEng = "";
                }
            }
            ProductModel pm = new ProductModel
            {
                Products = products
            };
            return View(pm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProductModel pm)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product
                {
                    Name = pm.Name,
                    Description = pm.Description,
                    Price = pm.Price
                };
                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();
            }
            List<Product> products = await _db.Products
                                                    .Take(5)
                                                        .ToListAsync();
            foreach (Product product in products)
            {
                var client = new RestClient("https://google-translate1.p.rapidapi.com/language/translate/v2");
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-rapidapi-host", "google-translate1.p.rapidapi.com");
                request.AddHeader("x-rapidapi-key", "e219260d74mshc9842dc6a717bd6p187982jsnddeece884433");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", $"source=az&q={product.Description}target=en", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(response.Content);
                    JToken data = json["data"];
                    JToken translations = data["translations"];
                    JToken firstTranslations = translations.First;
                    product.DescEng = firstTranslations["translatedText"].ToString();
                }
                else
                {
                    product.DescEng = "";
                }
            }
            pm.Products = products;
            return View(pm);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id != 0)
            {
                Product product = await _db.Products
                                                .Where(p => p.Id == id)
                                                    .FirstOrDefaultAsync();
                return View(product);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                Product dbProduct = await _db.Products
                                                .Where(p => p.Id == product.Id)
                                                    .FirstOrDefaultAsync();
                dbProduct.Name = product.Name;
                dbProduct.Description = product.Description;
                dbProduct.Price = product.Price;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id != 0)
            {
                Product product = await _db.Products
                                                .Where(p => p.Id == id)
                                                    .FirstOrDefaultAsync();
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}