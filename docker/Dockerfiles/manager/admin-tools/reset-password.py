import os, requests
from colorama import Fore, Style

# Set up environment variables for backend connection
API_URL = "http://backend:8080/api/Admin/users/reset-password"
DB_USERNAME = os.getenv('DB_USERNAME')
DB_PASSWORD = os.getenv('DB_PASSWORD')

def reset_password(dbUser, dbPassword, userEmail):
    payload = {
        "DbUsername": dbUser,
        "DbPassword": dbPassword,
        "UserEmail": userEmail
    }
    try:
        r = requests.post(API_URL, json=payload)
        if r.status_code == 404:
            print(Fore.YELLOW + "Not found detail: " + r.text + Style.RESET_ALL)
            return None
        r.raise_for_status()
        return r.json()
    except requests.HTTPError as e:
        print(Fore.RED + f"HTTP {e.response.status_code}: {e.response.text}" + Style.RESET_ALL)
    except requests.RequestException as e:
        print(Fore.RED + f"Request error: {e}" + Style.RESET_ALL)
    return None

def print_success(resp):
    title = " The password has been reset successfully! "
    border = "=" * len(title)
    print(Fore.GREEN + border)
    print(title)
    print(border + Style.RESET_ALL)
    print(Fore.CYAN + "Email       " + Fore.WHITE + ": " + Fore.MAGENTA + resp['userEmail'])
    print(Fore.CYAN + "User ID     " + Fore.WHITE + ": " + Fore.YELLOW + resp['userId'])
    print(Fore.CYAN + "New Password" + Fore.WHITE + ": " + Fore.MAGENTA + resp['userNewPassword'] + Style.RESET_ALL)
    print(Fore.GREEN + "-" * len(border) + Style.RESET_ALL)
    print(Fore.YELLOW + "Share the new password with the user securely." + Style.RESET_ALL)

if __name__ == "__main__":
    user_email = input("Enter the email of the user to reset password: ").strip()
    if not user_email:
        print(Fore.RED + "Email cannot be empty." + Style.RESET_ALL)
    else:
        response = reset_password(DB_USERNAME, DB_PASSWORD, user_email)
        if response:
            print_success(response)
        else:
            print(Fore.RED + "Failed to reset password." + Style.RESET_ALL)
