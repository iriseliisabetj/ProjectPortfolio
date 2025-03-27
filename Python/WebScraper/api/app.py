from flask import Flask, jsonify
import sqlite3

app = Flask(__name__)

def query_db(query, args=(), one=False):
    conn = sqlite3.connect("../scraper/products.db")
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    cur.execute(query, args)
    rv = cur.fetchall()
    conn.commit()
    conn.close()
    return (rv[0] if rv else None) if one else rv

@app.route("/products", methods=["GET"])
def get_all_products():
    products = query_db("SELECT * FROM products")
    return jsonify([dict(row) for row in products]), 200

@app.route("/products/above/<int:price>", methods=["GET"])
def get_products_above(price):
    products = query_db("SELECT * FROM products WHERE price > ?", (float(price),))
    return jsonify([dict(row) for row in products]), 200

if __name__ == "__main__":
    app.run(debug=True)
