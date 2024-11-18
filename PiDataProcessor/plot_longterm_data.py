import sqlite3
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from datetime import datetime

# Configure seaborn aesthetics
sns.set(style="whitegrid")

def fetch_data(db_path='longterm_analysis.db'):
    """
    Connects to the SQLite database and fetches data from the AggregatedData table.

    Args:
        db_path (str): Path to the SQLite database file.

    Returns:
        pd.DataFrame: DataFrame containing the aggregated data.
    """
    try:
        conn = sqlite3.connect(db_path)
        query = "SELECT * FROM AggregatedData"
        df = pd.read_sql_query(query, conn)
        conn.close()
        return df
    except sqlite3.Error as e:
        print(f"SQLite error: {e}")
        return pd.DataFrame()

def preprocess_data(df):
    """
    Preprocesses the DataFrame by converting date strings to datetime objects.

    Args:
        df (pd.DataFrame): The raw DataFrame.

    Returns:
        pd.DataFrame: The preprocessed DataFrame.
    """
    if df.empty:
        print("No data available to process.")
        return df

    # Convert 'date' column to datetime
    df['date'] = pd.to_datetime(df['date'])

    return df

def plot_average_welfare_score(df, output_dir='plots'):
    """
    Plots the average welfare score over time for each pig.

    Args:
        df (pd.DataFrame): The preprocessed DataFrame.
        output_dir (str): Directory to save the plots.
    """
    plt.figure(figsize=(12, 6))
    sns.lineplot(data=df, x='date', y='average_welfare_score', hue='pig_id', marker='o')
    plt.title('Average Welfare Score Over Time')
    plt.xlabel('Date')
    plt.ylabel('Average Welfare Score')
    plt.legend(title='Pig ID')
    plt.xticks(rotation=45)
    plt.tight_layout()
    plt.savefig(f"{output_dir}/average_welfare_score.png")
    plt.close()
    print("Saved plot: average_welfare_score.png")

def plot_total_distance_moved(df, output_dir='plots'):
    """
    Plots the total distance moved over time for each pig.

    Args:
        df (pd.DataFrame): The preprocessed DataFrame.
        output_dir (str): Directory to save the plots.
    """
    plt.figure(figsize=(12, 6))
    sns.barplot(data=df, x='date', y='total_distance_moved', hue='pig_id')
    plt.title('Total Distance Moved Over Time')
    plt.xlabel('Date')
    plt.ylabel('Total Distance Moved (units)')
    plt.legend(title='Pig ID')
    plt.xticks(rotation=45)
    plt.tight_layout()
    plt.savefig(f"{output_dir}/total_distance_moved.png")
    plt.close()
    print("Saved plot: total_distance_moved.png")

def plot_time_spent(df, output_dir='plots'):
    """
    Plots the time spent laying, standing, and moving over time for each pig.

    Args:
        df (pd.DataFrame): The preprocessed DataFrame.
        output_dir (str): Directory to save the plots.
    """
    metrics = ['time_laying', 'time_standing', 'time_moving']
    for metric in metrics:
        plt.figure(figsize=(12, 6))
        sns.lineplot(data=df, x='date', y=metric, hue='pig_id', marker='o')
        plt.title(f"{metric.replace('_', ' ').title()} Over Time")
        plt.xlabel('Date')
        plt.ylabel('Time (seconds)')
        plt.legend(title='Pig ID')
        plt.xticks(rotation=45)
        plt.tight_layout()
        plt.savefig(f"{output_dir}/{metric}.png")
        plt.close()
        print(f"Saved plot: {metric}.png")

def plot_keeper_presence(df, output_dir='plots'):
    """
    Plots the keeper presence duration over time for each pig.

    Args:
        df (pd.DataFrame): The preprocessed DataFrame.
        output_dir (str): Directory to save the plots.
    """
    plt.figure(figsize=(12, 6))
    sns.lineplot(data=df, x='date', y='keeper_presence_duration', hue='pig_id', marker='o')
    plt.title('Keeper Presence Duration Over Time')
    plt.xlabel('Date')
    plt.ylabel('Duration (seconds)')
    plt.legend(title='Pig ID')
    plt.xticks(rotation=45)
    plt.tight_layout()
    plt.savefig(f"{output_dir}/keeper_presence_duration.png")
    plt.close()
    print("Saved plot: keeper_presence_duration.png")



def main():
    # Fetch data from the database
    df = fetch_data()

    # Preprocess the data
    df = preprocess_data(df)

    if df.empty:
        print("No data to plot. Exiting.")
        return

    # Create a directory for plots if it doesn't exist
    import os
    output_dir = 'plots'
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    # Generate plots
    plot_average_welfare_score(df, output_dir)
    plot_total_distance_moved(df, output_dir)
    plot_time_spent(df, output_dir)
    plot_keeper_presence(df, output_dir)

    print("All plots have been generated and saved in the 'plots' directory.")

if __name__ == "__main__":
    main()
