import sqlite3
import csv

def import_csv_to_db(csv_filename):
    conn = sqlite3.connect("cafes.db")
    cursor = conn.cursor()

    with open(csv_filename, encoding='utf-8') as f:
        reader = csv.reader(f)
        for row in reader:
            name = row[0]
            location = row[1]
            time_open = row[3]
            time_closed = row[4]

            cursor.execute("""
                    INSERT INTO SOOKLA (name, location, time_open, time_closed)
                    VALUES (?, ?, ?, ?)
                """, (name, location, time_open, time_closed))

    conn.commit()
    conn.close()

if __name__ == "__main__":
    import_csv_to_db("kohvikud.csv")
    print("CSV andmed lisatud andmebaasi.")