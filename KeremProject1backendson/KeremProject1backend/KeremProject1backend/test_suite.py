import requests
import json
import time

BASE_URL = "http://localhost:5070/api"
ERROR_LOG = "test_errors.log"

def log_error(step, message, response=None):
    with open(ERROR_LOG, "a") as f:
        f.write(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] FAIL at {step}: {message}\n")
        if response:
            try:
                f.write(f"Response: {json.dumps(response.json(), indent=2)}\n")
            except:
                f.write(f"Response (Text): {response.text}\n")
        f.write("-" * 50 + "\n")
    print(f"❌ FAIL: {step} - Check {ERROR_LOG}")

def log_success(step):
    print(f"✅ PASS: {step}")

# 1. AUTHENTICATION TEST
def test_auth():
    print("\n--- Testing Authentication ---")
    
    # Register Manager (Admin of Institution)
    manager_email = f"manager_{int(time.time())}@test.com"
    manager_data = {
        "email": manager_email,
        "password": "Password123!",
        "fullName": "Test Manager",
        "phone": "5551234567"
    }
    
    # 1.1 Register
    try:
        res = requests.post(f"{BASE_URL}/auth/register-institution-manager", json=manager_data)
        if res.status_code == 200:
            log_success("Register Institution Manager")
            # Create Institution immediately? or implicit? 
            # Assuming registration creates pending institution request or user needs to create one.
            # Let's check login first.
        else:
            log_error("Register Manager", f"Status: {res.status_code}", res)
            return None
    except Exception as e:
        log_error("Register Manager", str(e))
        return None

    # 1.2 Login
    login_data = {
        "email": manager_email,
        "password": "Password123!"
    }
    try:
        res = requests.post(f"{BASE_URL}/auth/login", json=login_data)
        if res.status_code == 200:
            log_success("Login Manager")
            token = res.json().get("data", {}).get("token")
            return token
        else:
            log_error("Login Manager", f"Status: {res.status_code}", res)
            return None
    except Exception as e:
        log_error("Login Manager", str(e))
        return None

# 2. CLASSROOM TEST
def test_classroom(token):
    print("\n--- Testing Classroom ---")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Create Classroom
    classroom_data = {
        "institutionId": 1, # Assuming ID 1 exists or created by manager. 
        # Wait, manager needs Approved Institution. 
        # This might fail if Manager isn't linked to an approved institution.
        # Let's try creating one.
        "name": f"Test Class {int(time.time())}",
        "grade": 10
    }
    
    # Need to know Manager's Institution ID first? 
    # Or Manager creates request?
    # Let's try creating classroom directly (if manager has created institution during register flow or seeding)
    
    # Simplified flow assuming SEEDED ADMIN/MANAGER for robust testing
    # Actually, let's use the Seeded Admin to Approve things if needed.
    
    try:
        res = requests.post(f"{BASE_URL}/classroom", json=classroom_data, headers=headers)
        if res.status_code == 200:
            log_success("Create Classroom")
            class_id = res.json().get("data")
            return class_id
        else:
            log_error("Create Classroom", f"Status: {res.status_code}", res)
            return None
    except Exception as e:
        log_error("Create Classroom", str(e))
        return None

# 3. MESSAGE TEST
def test_message(token, class_id):
    print("\n--- Testing Message ---")
    headers = {"Authorization": f"Bearer {token}"}
    
    if not class_id:
        print("Skipping Message Test (No Classroom ID)")
        return

    # Send Message to Class (Automatic Group Created?)
    # Need Conversation ID. How do I get it?
    # Classroom creation response returns ID. 
    # Does it return conversation ID? No, usually separate.
    # Let's assume we can get conversation by Classroom ID or just try generic message send endpoint if exists.
    
    # Endpoint: POST /api/message/send
    # Needs conversationId.
    # We need to GET classroom details to find conversation ID?
    
    try:
        # Get Classroom Details (hoping it has Conversation Info)
        res = requests.get(f"{BASE_URL}/classroom/{class_id}", headers=headers)
        if res.status_code == 200:
            class_data = res.json().get("data")
            # conversationId might be in data? Or we assume a convention?
            # Creating classroom creates a Conversation. We need its ID.
            # If API doesn't return it, we might need a specific endpoint like "/api/message/conversations"
            pass
        else:
            log_error("Get Classroom Details", f"Status: {res.status_code}", res)
    except:
        pass

    # For now, let's skip specific message sending if we can't easily get ID in one step.
    # But user specifically asked for Message Test.
    # I'll check /api/message/conversations to find the group.
    
    try:
        res = requests.get(f"{BASE_URL}/message/conversations", headers=headers)
        if res.status_code == 200:
            convs = res.json().get("data", [])
            if convs:
                target_conv = convs[0]['id'] # Pick first one
                
                msg_data = {
                    "conversationId": target_conv,
                    "content": "Automated Test Message",
                    "type": 0
                }
                
                res_msg = requests.post(f"{BASE_URL}/message/send", json=msg_data, headers=headers)
                if res_msg.status_code == 200:
                    log_success("Send Message (SignalR Trigger)")
                else:
                    log_error("Send Message", f"Status: {res_msg.status_code}", res_msg)
            else:
                log_error("Get Conversations", "No conversations found for user")
        else:
            log_error("Get Conversations", f"Status: {res.status_code}", res)
    except Exception as e:
        log_error("Message Flow", str(e))


def main():
    # Setup Error Log
    with open(ERROR_LOG, "w") as f:
        f.write("Test Execution Log\n==================\n")

    # Run Tests
    token = test_auth()
    
    if token:
        class_id = test_classroom(token)
        test_message(token, class_id)
    
    print(f"\nTests Completed. Check {ERROR_LOG} for failures.")

if __name__ == "__main__":
    main()
