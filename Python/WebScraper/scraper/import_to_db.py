import sqlite3
import json

def insert_data():
    conn = sqlite3.connect("products.db")
    cursor = conn.cursor()

    with open("products.json", "r", encoding="utf-8") as f:
        products = json.load(f)

    for product in products:
        cursor.execute("""
            INSERT INTO products (title, price, image, product_url)
            VALUES (?, ?, ?, ?)
        """, (product["Title"], product["Price"], product["Image"], product["Product URL"]))

    conn.commit()
    conn.close()

if __name__ == "__main__":
    insert_data()
    print("Products inserted into database.")
