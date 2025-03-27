import sqlite3

def create_database():
    conn = sqlite3.connect("cafes.db")
    cursor = conn.cursor()

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS SOOKLA (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            location TEXT,
            time_open TEXT,
            time_closed TEXT
        )
    """)

    conn.commit()
    conn.close()

if __name__ == "__main__":
    create_database()
    print("Andmebaas loodud/olemas ja tabel SOOKLA valmis.")