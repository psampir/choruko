import os, requests, json
from colorama import Fore, Style

# Set up environment variables for backend connection
API_URL = "http://backend:8080/api/Admin/users/reset-password"
DB_USERNAME = os.getenv('DB_USERNAME')
DB_PASSWORD = os.getenv('DB_PASSWORD')

def print_error(status_code, response_text):
    """Pretty print error responses from the API"""
    try:
        error_data = json.loads(response_text)
        
        # Extract error messages
        errors = error_data.get('errors', {})
        title = error_data.get('title', 'Error')
        
        print(Fore.RED + "╔" + "═" * 60 + "╗")
        print("║ " + Fore.YELLOW + f"Error {status_code}" + Fore.RED + " " * (59 - len(f"Error {status_code}")) + "║")
        print("╠" + "═" * 60 + "╣")
        
        if errors:
            for field, messages in errors.items():
                if isinstance(messages, list):
                    for msg in messages:
                        print("║ " + Fore.WHITE + f"• {msg}" + Fore.RED + " " * (59 - len(f"• {msg}")) + "║")
                else:
                    print("║ " + Fore.WHITE + f"• {messages}" + Fore.RED + " " * (59 - len(f"• {messages}")) + "║")
        else:
            print("║ " + Fore.WHITE + title + Fore.RED + " " * (59 - len(title)) + "║")
        
        print("╚" + "═" * 60 + "╝" + Style.RESET_ALL)
    except json.JSONDecodeError:
        # Fallback for non-JSON responses
        print(Fore.RED + "╔" + "═" * 60 + "╗")
        print("║ " + Fore.YELLOW + f"Error {status_code}" + Fore.RED + " " * (59 - len(f"Error {status_code}")) + "║")
        print("╠" + "═" * 60 + "╣")
        print("║ " + Fore.WHITE + response_text[:58] + Fore.RED + " " * max(0, 59 - len(response_text[:58])) + "║")
        print("╚" + "═" * 60 + "╝" + Style.RESET_ALL)

def reset_password(dbUser, dbPassword, userEmail):
    payload = {
        "DbUsername": dbUser,
        "DbPassword": dbPassword,
        "UserEmail": userEmail
    }
    try:
        r = requests.post(API_URL, json=payload)
        if r.status_code in [400, 404]:
            print_error(r.status_code, r.text)
            return None
        r.raise_for_status()
        return r.json()
    except requests.HTTPError as e:
        print_error(e.response.status_code, e.response.text)
    except requests.RequestException as e:
        print(Fore.RED + f"╔{'═' * 60}╗")
        print(f"║ Connection Error{' ' * 46}║")
        print(f"╠{'═' * 60}╣")
        print(f"║ {str(e)[:58]}{' ' * max(0, 59 - len(str(e)[:58]))}║")
        print(f"╚{'═' * 60}╝" + Style.RESET_ALL)
    return None

def print_success(resp):
    title = " The password has been reset successfully! "
    border = "=" * len(title)
    print(Fore.GREEN + border)
    print(title)
    print(border + Style.RESET_ALL)
    # ASP.NET Core returns camelCase by default in the 'result' field
    result = resp.get('result', resp)  # Handle both wrapped and unwrapped responses
    print(Fore.CYAN + "Email       " + Fore.WHITE + ": " + Fore.MAGENTA + result['userEmail'])
    print(Fore.CYAN + "User ID     " + Fore.WHITE + ": " + Fore.YELLOW + result['userId'])
    print(Fore.CYAN + "New Password" + Fore.WHITE + ": " + Fore.MAGENTA + result['userNewPassword'] + Style.RESET_ALL)
    print(Fore.GREEN + "-" * len(border) + Style.RESET_ALL)
    print(Fore.YELLOW + "Share the new password with the user securely." + Style.RESET_ALL)

if __name__ == "__main__":
    user_email = input("Enter the email of the user to reset password: ").strip()
    if not user_email:
        print(Fore.RED + "╔" + "═" * 60 + "╗")
        print("║ " + Fore.YELLOW + "Validation Error" + Fore.RED + " " * 44 + "║")
        print("╠" + "═" * 60 + "╣")
        print("║ " + Fore.WHITE + "• Email cannot be empty." + Fore.RED + " " * 35 + "║")
        print("╚" + "═" * 60 + "╝" + Style.RESET_ALL)
    else:
        response = reset_password(DB_USERNAME, DB_PASSWORD, user_email)
        if response:
            print_success(response)
