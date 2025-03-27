import requests
from bs4 import BeautifulSoup
import json
import re

BASE_URL = "https://www.smartech.ee"
CATEGORY_URL = "https://www.smartech.ee/et/category/telefonid-nutikellad-tahvelarvutid-ja-e-lugerid/tahvelarvutid-graafikatahvlid-e-lugerid-ja-tarvikud/e-lugerid-ja-lisad/"

def scrape_page(page=1, all_products=[]):
    url = f"{CATEGORY_URL}?page={page}&orderPrice="
    print(f"Scraping page {page}: {url}")

    response = requests.get(url)
    soup = BeautifulSoup(response.text, "html.parser")

    products = []
    for item in soup.find_all("a", class_="productlist-item-body"):
        name = item.find("strong").text.strip()
        raw_price = item.find("em").text.strip() if item.find("em") else "N/A"
        price_match = re.search(r"(\d+[\.,]?\d*)", raw_price)
        price = float(price_match.group(1).replace(",", ".")) if price_match else None

        figure = item.find("figure")
        if figure and figure.find("img"):
            img_tag = figure.find("img")
            image = img_tag.get("data-src", img_tag.get("src", "N/A"))
        else:
            image = "N/A"

        if "/img/noimage.png" in image:
            image = "N/A"

        product_url = BASE_URL + item["href"]

        products.append({
            "Title": name,
            "Price": price,
            "Image": image,
            "Product URL": product_url
        })

    if not products:
        print(f"No more products found on page {page}. Stopping recursion.")
        return all_products

    all_products.extend(products)

    return scrape_page(page + 1, all_products)

all_products = scrape_page()

with open("products.json", "w", encoding="utf-8") as f:
    json.dump(all_products, f, indent=4, ensure_ascii=False)

print(f"Scraped {len(all_products)} products across multiple pages!")
