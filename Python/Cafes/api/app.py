from flask import Flask, request, jsonify
import sqlite3

app = Flask(__name__)

def query_db(query, args=(), one=False):
    conn = sqlite3.connect("cafes.db")
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    cur.execute(query, args)
    rv = cur.fetchall()
    conn.commit()
    conn.close()
    return (rv[0] if rv else None) if one else rv

@app.route("/api/cafes", methods=["GET"])
def get_all_cafes():
    rows = query_db("SELECT * FROM SOOKLA")
    result = []
    for row in rows:
        result.append({
            "id": row["id"],
            "name": row["name"],
            "location": row["location"],
            "time_open": row["time_open"],
            "time_closed": row["time_closed"]
        })
    return jsonify(result), 200


@app.route("/api/cafes/open", methods=["GET"])
def get_cafes_by_time():
    start = request.args.get("start", "")
    end = request.args.get("end", "")

    if not start or not end:
        rows = query_db("SELECT * FROM SOOKLA")
    else:
        query = """
            SELECT * FROM SOOKLA
            WHERE time_open <= ?
              AND time_closed >= ?
        """
        rows = query_db(query, [start, end])

    result = []
    for row in rows:
        result.append({
            "id": row["id"],
            "name": row["name"],
            "location": row["location"],
            "time_open": row["time_open"],
            "time_closed": row["time_closed"]
        })
    return jsonify(result), 200

@app.route("/api/cafes", methods=["POST"])
def add_cafe():
    data = request.json
    name = data.get("name")
    location = data.get("location")
    time_open = data.get("time_open")
    time_closed = data.get("time_closed")

    query_db("""
        INSERT INTO SOOKLA (name, location, time_open, time_closed)
        VALUES (?, ?, ?, ?)
    """, (name, location, time_open, time_closed))

    return jsonify({"message": "New cafe added"}), 201

@app.route("/api/cafes/<int:cafe_id>", methods=["GET"])
def get_one_cafe(cafe_id):
    row = query_db("SELECT * FROM SOOKLA WHERE id = ?", [cafe_id], one=True)
    if row:
        return {
            "id": row["id"],
            "name": row["name"],
            "location": row["location"],
            "time_open": row["time_open"],
            "time_closed": row["time_closed"]
        }, 200
    else:
        return {"error": "Not found"}, 404

@app.route("/api/cafes/<int:cafe_id>", methods=["PUT"])
def update_cafe(cafe_id):
    data = request.json
    name = data.get("name")
    location = data.get("location")
    time_open = data.get("time_open")
    time_closed = data.get("time_closed")

    query_db("""
        UPDATE SOOKLA
        SET name = ?, location = ?, time_open = ?, time_closed = ?
        WHERE id = ?
    """, (name, location, time_open, time_closed, cafe_id))

    return jsonify({"message": f"Cafe {cafe_id} updated"}), 200

@app.route("/api/cafes/<int:cafe_id>", methods=["DELETE"])
def delete_cafe(cafe_id):
    query_db("DELETE FROM SOOKLA WHERE id = ?", [cafe_id])
    return jsonify({"message": f"Cafe {cafe_id} deleted"}), 200

if __name__ == "__main__":
    app.run(debug=True)
