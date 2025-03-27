import sqlite3

def create_database():
    conn = sqlite3.connect("products.db")
    cursor = conn.cursor()

    cursor.execute("""
        CREATE TABLE IF NOT EXISTS products (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT,
            price REAL NOT NULL,
            image TEXT,
            product_url TEXT
        )
    """)

    conn.commit()
    conn.close()

if __name__ == "__main__":
    create_database()
    print("Database created successfully.")
