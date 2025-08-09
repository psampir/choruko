import json
import os
import subprocess
import sys
from pathlib import Path

# This script reads database connection details from launchSettings.json
# and runs the 'dotnet ef database update' command.

def main():
    """Main function to execute the script logic."""
    
    # --- Configuration ---
    # Get the script's directory to build absolute paths.
    # The script is located in the 'backend' directory.
    script_dir = Path(__file__).parent.resolve()
    launch_settings_path = script_dir / "ChorukoAPI" / "Properties" / "launchSettings.json"
    api_project_path = script_dir / "ChorukoAPI"

    # --- Pre-flight Checks ---
    if not launch_settings_path.is_file():
        print(f"Error: launchSettings.json not found at {launch_settings_path}")
        sys.exit(1)

    # --- Main Script ---
    print(f"Reading environment variables from {launch_settings_path}...")

    try:
        with open(launch_settings_path) as f:
            settings = json.load(f)
        
        # Extract the environment variables from the 'http' profile
        env_vars = settings['profiles']['http']['environmentVariables']
    except (json.JSONDecodeError, KeyError) as e:
        print(f"Error parsing launchSettings.json: {e}")
        sys.exit(1)

    # Get a copy of the current environment and update it with variables from the file
    script_env = os.environ.copy()
    script_env.update(env_vars)

    # The command to run, including any arguments passed to this python script
    command = ["dotnet", "ef", "database", "update"] + sys.argv[1:]

    print(f"Running command: {' '.join(command)}")
    print(f"In directory: {api_project_path}")
    print("-" * 20)

    # Run the command using the updated environment and correct working directory
    result = subprocess.run(command, env=script_env, cwd=api_project_path)
    
    print("-" * 20)
    print("Database update command finished.")
    
    # Exit with the same return code as the executed command
    sys.exit(result.returncode)

if __name__ == "__main__":
    main()