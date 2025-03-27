from flask import Flask, render_template, request
import requests

app = Flask(__name__)

API_URL = "http://127.0.0.1:5000"


@app.route("/")
def index():
    resp = requests.get(f"{API_URL}/products")
    if resp.status_code == 200:
        products = resp.json()
    else:
        products = []

    return render_template("index.html", products=products)


@app.route("/filter", methods=["POST"])
def filter_products():
    price = request.form.get("price")

    if price:
        resp = requests.get(f"{API_URL}/products/above/{price}")
        if resp.status_code == 200:
            products = resp.json()
        else:
            products = []
    else:
        return index()

    return render_template("filter.html", products=products, price=price)


if __name__ == "__main__":
    app.run(port=5001, debug=True)
