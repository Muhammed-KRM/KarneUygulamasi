
import requests
import json
import time

BASE_URL = "http://localhost:5070"
REPORT_FILE = "test_report.txt"

def log(message):
    print(message)
    with open(REPORT_FILE, "a", encoding="utf-8") as f:
        f.write(message + "\n")

def run_test(name, method, endpoint, payload=None, token=None, expected_status=200):
    url = f"{BASE_URL}{endpoint}"
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    
    log(f"\n--- TEST: {name} ---")
    log(f"Request: {method} {url}")
    if payload:
        log(f"Payload: {json.dumps(payload, indent=2)}")
    
    try:
        if method == "POST":
            response = requests.post(url, json=payload, headers=headers)
        elif method == "GET":
            response = requests.get(url, headers=headers)
        elif method == "PUT":
            response = requests.put(url, json=payload, headers=headers)
        elif method == "DELETE":
            response = requests.delete(url, headers=headers)
        else:
            log("Invalid method")
            return None

        log(f"Response Status: {response.status_code}")
        try:
            json_resp = response.json()
            log(f"Response Body: {json.dumps(json_resp, indent=2)}")
            return response, json_resp
        except:
            log(f"Response Body: {response.text}")
            return response, None
    except Exception as e:
        log(f"EXCEPTION: {str(e)}")
        return None, None

def main():
    # Clear report file
    with open(REPORT_FILE, "w", encoding="utf-8") as f:
        f.write("COMPREHENSIVE INSTITUTION WORKFLOW TEST REPORT\n")
        f.write("================================================\n")

    timestamp = int(time.time())
    owner_email = f"owner_{timestamp}@test.com"
    owner_user = f"owner_{timestamp}"
    password = "Password123!"

    # 1. Register Owner
    payload_reg = {
        "fullName": "Test Owner",
        "username": owner_user,
        "email": owner_email,
        "password": password,
        "registerAsOwner": True
    }
    resp, json_data = run_test("Register Owner", "POST", "/api/auth/register", payload_reg)
    if not resp or resp.status_code != 200:
        log("FAILED: Registration")
        return

    # 2. Login
    payload_login = {
        "username": owner_user,
        "password": password
    }
    resp, json_data = run_test("Login Owner", "POST", "/api/auth/login", payload_login)
    if not resp or resp.status_code != 200:
        log("FAILED: Login")
        return
    
    token = json_data["data"]["token"]
    user_id = json_data["data"]["user"]["id"]
    log(f"Logged in as UserID: {user_id}")

    # 3. Apply Institution
    # Assumption based on finding: /api/auth/apply-institution or /api/institution/apply
    # I will verify this in next steps but assuming standard naming based on previous code exploration
    # If this fails, I'll adjust. 
    # Let's try /api/auth/apply-institution first as it was in AuthOperations
    payload_inst = {
        "name": f"Test Inst {timestamp}",
        "licenseNumber": f"LIC{timestamp}",
        "address": "123 Test St",
        "phone": "5551234567"
    }
    resp, json_data = run_test("Apply Institution", "POST", "/api/auth/apply-institution", payload_inst, token)
    
    # 4. Get My Institutions to find ID
    resp, json_data = run_test("Get My Institutions", "GET", "/api/institution/my-institutions", None, token)
    if not resp or resp.status_code != 200:
        log("FAILED: Get My Institutions")
        return

    institutions = json_data.get("data", [])
    if not institutions:
        log("FAILED: No institutions found")
        return
    
    inst_id = institutions[0]["id"]
    log(f"Target Institution ID: {inst_id}")

    # 5. Create Manager
    manager_email = f"manager_{timestamp}@test.com"
    payload_mgr = {
        "fullName": "Test Manager",
        "email": manager_email,
        "password": "Password123!",
        "phone": "5559876543"
    }
    resp, json_data = run_test("Create Manager", "POST", f"/api/institution/{inst_id}/managers", payload_mgr, token)
    
    manager_id = None
    if resp and resp.status_code == 200:
        manager_id = json_data["data"] # Assuming returns ID
        log(f"Created Manager ID: {manager_id}")

    # 6. List Managers
    resp, json_data = run_test("List Managers", "GET", f"/api/institution/{inst_id}/members?role=Manager", None, token)

    # 7. Update Manager
    if manager_id:
        payload_update = {
            "fullName": "Updated Manager Name",
            "email": f"updated_{manager_email}",
            "phone": "5550001111"
        }
        # Note: DTO might be different, checking previous context UpdateUserRequest
        run_test("Update Manager", "PUT", f"/api/institution/{inst_id}/managers/{manager_id}", payload_update, token)

    # 8. Delete Manager
    if manager_id:
        run_test("Delete Manager", "DELETE", f"/api/institution/{inst_id}/managers/{manager_id}", None, token)

    # Summary
    log("\n=== SUMMARY ===")
    log("Check above for Failed/Success steps.")

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        log(f"FATAL ERROR: {e}")
