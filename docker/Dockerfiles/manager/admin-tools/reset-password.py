import os
import requests
from colorama import Fore, Style

# Set up environment variables for backend connection
dbUser = os.getenv('DB_USERNAME')
dbPassword = os.getenv('DB_PASSWORD')
apiUrl = "http://backend:8080/api/admin/resetPassword"

# Function to reset password
def reset_password(dbUser, dbPassword, userEmail):
    try:
        response = requests.post(apiUrl, json={
            "dbUser": dbUser,
            "dbPassword": dbPassword,
            "email": userEmail
        })
        response.raise_for_status()  # Raise an error for HTTP errors
        return response.json().get("newPassword")
    except requests.RequestException as e:
        print(Fore.RED + "Error resetting password:" + Style.RESET_ALL, e)
        return None

if __name__ == "__main__":
    userEmail = input("Enter the email of the user to reset password: ").strip()
    if not userEmail:
        print(Fore.RED + "Email cannot be empty." + Style.RESET_ALL)
    else:
        newPassword = reset_password(dbUser, dbPassword, userEmail)
        if newPassword:
            print(Fore.GREEN + f"Password for {userEmail} has been reset to: {newPassword}" + Style.RESET_ALL)
        else:
            print(Fore.RED + "Failed to reset password." + Style.RESET_ALL)
    