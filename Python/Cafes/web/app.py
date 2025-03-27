from flask import Flask, render_template, request, redirect, url_for
import requests

app = Flask(__name__)

API_URL = "http://127.0.0.1:5000"

@app.route("/")
def index():
    resp = requests.get(f"{API_URL}/api/cafes")
    cafes = resp.json()
    return render_template("index.html", cafes=cafes)

@app.route("/search_open", methods=["POST"])
def search_open():
    start = request.form.get("start")
    end = request.form.get("end")
    resp = requests.get(f"{API_URL}/api/cafes/open", params={"start": start, "end": end})
    cafes = resp.json()
    return render_template("index.html", cafes=cafes, start=start, end=end)

@app.route("/add_cafe", methods=["POST"])
def add_cafe():
    name = request.form.get("name")
    location = request.form.get("location")
    time_open = request.form.get("time_open")
    time_closed = request.form.get("time_closed")

    requests.post(f"{API_URL}/api/cafes", json={
        "name": name,
        "location": location,
        "time_open": time_open,
        "time_closed": time_closed
    })
    return redirect(url_for("index"))

@app.route("/edit_cafe/<int:cafe_id>", methods=["GET", "POST"])
def edit_cafe(cafe_id):
    if request.method == "GET":
        resp = requests.get(f"{API_URL}/api/cafes/{cafe_id}")
        if resp.status_code == 200:
            cafe = resp.json()
            return render_template("edit_cafe.html", cafe=cafe)
        else:
            return f"Kohvikut ID={cafe_id} ei leitud", 404

    elif request.method == "POST":
        name = request.form.get("name")
        location = request.form.get("location")
        time_open = request.form.get("time_open")
        time_closed = request.form.get("time_closed")

        requests.put(f"{API_URL}/api/cafes/{cafe_id}", json={
            "name": name,
            "location": location,
            "time_open": time_open,
            "time_closed": time_closed
        })

        return redirect(url_for("index"))

@app.route("/delete_cafe/<int:cafe_id>", methods=["GET"])
def delete_cafe(cafe_id):
    requests.delete(f"{API_URL}/api/cafes/{cafe_id}")
    return redirect(url_for("index"))

if __name__ == "__main__":
    app.run(port=5001, debug=True)
