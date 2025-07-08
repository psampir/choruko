import os
from colorama import Fore, Style

# Get the variables from environment
name = os.getenv("name", "Unknown")
age = os.getenv("age", "0")

# Assemble and print the message
message = f"{Fore.GREEN}{name}{Style.RESET_ALL} is {Fore.BLUE}{age}{Style.RESET_ALL} years old"
print(message)
