"""
Kapsamlı Senaryo Testi
======================
Bu test, sistemin tam akışını test eder:
1. AdminAdmin login → Admin oluştur → Logout
2. 2 Dershane sahibi kayıt → Login → Kurum başvurusu → Logout
3. Admin login → Başvuruları onayla → Logout
4. Sahip1 login → Sahip2'yi co-owner ekle → Logout
5. Sahip2 login → Kurumları listele → Logout
6. Sahip2 → 2 Yönetici oluştur → Logout
7. Yönetici1 → 5 öğretmen, 2 sınıf, öğretmen atama, 10 öğrenci → Logout
8. Öğretmen → Sınav oluştur → Optik işle → Karne → Sınıfa paylaş → Logout
9. Öğrenci → Karne kontrol → Mesaj yaz → Logout
"""

import requests
import json
import time
import os
import uuid
from datetime import datetime

BASE_URL = "http://localhost:5070/api"
REPORT_FILE = "scenario_test_results.txt"
OPTICAL_OUTPUT_DIR = "optical_files"

# Test Data Storage
test_data = {
    "admin_admin_token": None,
    "admin_token": None,
    "admin_id": None,
    "owner1_token": None,
    "owner1_id": None,
    "owner1_email": None,
    "owner2_token": None,
    "owner2_id": None,
    "owner2_email": None,
    "institution1_id": None,
    "institution2_id": None,
    "manager1_token": None,
    "manager1_id": None,
    "manager2_id": None,
    "teacher_tokens": [],
    "teacher_ids": [],
    "student_ids": [],
    "classroom1_id": None,
    "classroom2_id": None,
    "exam_id": None,
    "exam_results": [],
}

results = []

def log_result(step_name, success, message="", details=None):
    """Log test result"""
    status = "SUCCESS" if success else "FAIL"
    result = {
        "step": step_name,
        "status": status,
        "message": message,
        "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    }
    if details:
        result["details"] = details
    results.append(result)
    
    # Use simple text instead of emojis for Windows console compatibility
    symbol = "[OK]" if success else "[!!]"
    print(f"{symbol} {step_name}: {status} - {message}")
    return success

def save_report():
    """Save test results to file"""
    with open(REPORT_FILE, "w", encoding="utf-8") as f:
        f.write("="*60 + "\n")
        f.write("KAPSAMLI SENARYO TEST RAPORU\n")
        f.write(f"Tarih: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write("="*60 + "\n\n")
        
        passed = sum(1 for r in results if r["status"] == "SUCCESS")
        failed = sum(1 for r in results if r["status"] == "FAIL")
        
        f.write(f"OZET: {passed} Basarili / {failed} Basarisiz / Toplam {len(results)}\n\n")
        f.write("-"*60 + "\n")
        
        for r in results:
            symbol = "[OK]" if r["status"] == "SUCCESS" else "[!!]"
            f.write(f"\n{symbol} [{r['timestamp']}] {r['step']}\n")
            f.write(f"   Status: {r['status']}\n")
            if r.get("message"):
                f.write(f"   Message: {r['message']}\n")
            if r.get("details"):
                f.write(f"   Details: {json.dumps(r['details'], ensure_ascii=False, indent=4)}\n")
        
        f.write("\n" + "="*60 + "\n")
        f.write("TEST TAMAMLANDI\n")
    
    print(f"\nRapor kaydedildi: {REPORT_FILE}")

def api_call(method, endpoint, data=None, token=None, files=None):
    """Make API call with error handling"""
    url = f"{BASE_URL}/{endpoint}"
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    
    try:
        if method == "GET":
            response = requests.get(url, headers=headers, params=data)
        elif method == "POST":
            if files:
                response = requests.post(url, headers=headers, files=files)
            else:
                headers["Content-Type"] = "application/json"
                response = requests.post(url, headers=headers, json=data)
        elif method == "PUT":
            headers["Content-Type"] = "application/json"
            response = requests.put(url, headers=headers, json=data)
        elif method == "DELETE":
            response = requests.delete(url, headers=headers)
        else:
            return None, "Invalid method"
        
        return response, None
    except Exception as e:
        return None, str(e)

# ============================================================================
# STEP 1: AdminAdmin Login, Create Admin, Logout
# ============================================================================
def step1_admin_operations():
    print("\n" + "="*50)
    print("ADIM 1: AdminAdmin İşlemleri")
    print("="*50)
    
    # 1.1 AdminAdmin Login
    login_data = {
        "username": "admin",
        "password": "Admin123!"
    }
    
    response, error = api_call("POST", "auth/login", login_data)
    if error or response.status_code != 200:
        log_result("1.1 AdminAdmin Login", False, f"Login failed: {error or response.text}")
        return False
    
    try:
        data = response.json()
        test_data["admin_admin_token"] = data.get("data", {}).get("token")
        if not test_data["admin_admin_token"]:
            log_result("1.1 AdminAdmin Login", False, "Token not received")
            return False
        log_result("1.1 AdminAdmin Login", True, "Token received")
    except:
        log_result("1.1 AdminAdmin Login", False, "Failed to parse response")
        return False
    
    # 1.2 Create Admin
    test_uid = str(uuid.uuid4())[:8]
    admin_email = f"testadmin_{test_uid}@test.com"
    admin_data = {
        "fullName": "Test Admin",
        "username": admin_email,
        "email": admin_email,
        "password": "TestAdmin123!"
    }
    
    response, error = api_call("POST", "admin/create-admin", admin_data, test_data["admin_admin_token"])
    if error or response.status_code != 200:
        log_result("1.2 Admin Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    try:
        data = response.json()
        test_data["admin_id"] = data.get("data")
        test_data["admin_email"] = admin_email
        log_result("1.2 Admin Oluştur", True, f"AdminId: {test_data['admin_id']}")
    except:
        log_result("1.2 Admin Oluştur", False, "Failed to parse response")
        return False
    
    # 1.3 Logout (conceptual - just clear token)
    test_data["admin_admin_token"] = None
    log_result("1.3 AdminAdmin Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 2: Register 2 Institution Owners, Apply for Institution
# ============================================================================
def step2_owner_registration():
    print("\n" + "="*50)
    print("ADIM 2: Dershane Sahipleri Kayıt ve Başvuru")
    print("="*50)
    
    timestamp = int(time.time())
    
    # 2.1 Register Owner 1
    owner1_email = f"owner1_{timestamp}@test.com"
    owner1_data = {
        "fullName": "Dershane Sahibi 1",
        "username": owner1_email,
        "email": owner1_email,
        "password": "Owner123!",
        "phone": "5551111111",
        "registerAsOwner": True
    }
    
    response, error = api_call("POST", "auth/register", owner1_data)
    if error or response.status_code != 200:
        log_result("2.1 Sahip1 Kayıt", False, f"Failed: {error or response.text}")
        return False
    
    log_result("2.1 Sahip1 Kayıt", True, "Registration successful")
    test_data["owner1_email"] = owner1_email
    
    # 2.2 Register Owner 2
    owner2_email = f"owner2_{timestamp}@test.com"
    owner2_data = {
        "fullName": "Dershane Sahibi 2",
        "username": owner2_email,
        "email": owner2_email,
        "password": "Owner123!",
        "phone": "5552222222",
        "registerAsOwner": True
    }
    
    response, error = api_call("POST", "auth/register", owner2_data)
    if error or response.status_code != 200:
        log_result("2.2 Sahip2 Kayıt", False, f"Failed: {error or response.text}")
        return False
        
    log_result("2.2 Sahip2 Kayıt", True, "Registration successful")
    test_data["owner2_email"] = owner2_email
    
    # 2.3 Owner 1 Login
    response, error = api_call("POST", "auth/login", {"username": owner1_email, "password": "Owner123!"})
    if error or response.status_code != 200:
        log_result("2.3 Sahip1 Login", False, f"Failed: {error or response.text}")
        return False
    
    data = response.json().get("data", {})
    test_data["owner1_token"] = data.get("token")
    test_data["owner1_id"] = data.get("user", {}).get("id")
    log_result("2.3 Sahip1 Login", True, f"Token received, UserId: {test_data['owner1_id']}")
    
    # 2.4 Owner 1 Apply for Institution
    inst1_data = {
        "name": f"Test Dershanesi 1 - {timestamp}",
        "licenseNumber": f"LIC-{timestamp}-001",
        "address": "Test Adres 1, İstanbul",
        "phone": "2121111111",
        "managerName": "Dershane Sahibi 1",
        "managerEmail": owner1_email
    }
    
    response, error = api_call("POST", "auth/apply-institution", inst1_data, test_data["owner1_token"])
    if error or response.status_code != 200:
        log_result("2.4 Sahip1 Kurum Başvurusu", False, f"Failed: {error or response.text}")
        return False
    
    test_data["inst1_name"] = inst1_data["name"]
    log_result("2.4 Sahip1 Kurum Başvurusu", True, "Application submitted (will fetch ID in admin step)")
    
    test_data["owner1_token"] = None
    log_result("2.5 Sahip1 Logout", True, "Token cleared")
    
    # 2.6 Owner 2 Login
    response, error = api_call("POST", "auth/login", {"username": owner2_email, "password": "Owner123!"})
    if error or response.status_code != 200:
        log_result("2.6 Sahip2 Login", False, f"Failed: {error or response.text}")
        return False
    
    data = response.json().get("data", {})
    test_data["owner2_token"] = data.get("token")
    test_data["owner2_id"] = data.get("user", {}).get("id")
    log_result("2.6 Sahip2 Login", True, f"Token received, UserId: {test_data['owner2_id']}")
    
    # 2.7 Owner 2 Apply for Institution
    inst2_data = {
        "name": f"Test Dershanesi 2 - {timestamp}",
        "licenseNumber": f"LIC-{timestamp}-002",
        "address": "Test Adres 2, Ankara",
        "phone": "3122222222",
        "managerName": "Dershane Sahibi 2",
        "managerEmail": owner2_email
    }
    
    response, error = api_call("POST", "auth/apply-institution", inst2_data, test_data["owner2_token"])
    if error or response.status_code != 200:
        log_result("2.7 Sahip2 Kurum Başvurusu", False, f"Failed: {error or response.text}")
        return False
    
    test_data["inst2_name"] = inst2_data["name"]
    log_result("2.7 Sahip2 Kurum Başvurusu", True, "Application submitted (will fetch ID in admin step)")
    
    test_data["owner2_token"] = None
    log_result("2.8 Sahip2 Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 3: Admin Approves Institutions
# ============================================================================
def step3_admin_approvals():
    print("\n" + "="*50)
    print("ADIM 3: Admin Kurum Onayları")
    print("="*50)
    
    # 3.1 AdminAdmin Login
    response, error = api_call("POST", "auth/login", {
        "username": "admin",
        "password": "Admin123!"
    })
    
    if error or response.status_code != 200:
        log_result("3.1 Admin Login", False, f"Failed: {error or response.text}")
        return False
        
    admin_token = response.json().get("data", {}).get("token")
    test_data["admin_token"] = admin_token
    log_result("3.1 Admin Login", True, "Token received")
    
    # 3.2 List Pending Institutions
    headers = {"Authorization": f"Bearer {admin_token}"}
    response = requests.get(f"{BASE_URL}/admin/institutions?status=PendingApproval", headers=headers)
    
    if response.status_code != 200:
        log_result("3.2 Kurumları Listele", False, f"Failed: {response.text}")
        return False
        
    pending_list = response.json().get("data", {}).get("items", [])
    
    # Find and Approve Institution 1
    inst1_name = test_data.get("inst1_name")
    inst1 = next((i for i in pending_list if i["name"] == inst1_name), None)
    
    if inst1:
        inst1_id = inst1["id"]
        test_data["institution1_id"] = inst1_id
        requests.post(f"{BASE_URL}/admin/institution/{inst1_id}/approve", headers=headers)
        log_result("3.2 Kurum1 Onay", True, f"Approved Institution1 ({inst1_id})")
    else:
        log_result("3.2 Kurum1 Onay", False, f"Institution1 '{inst1_name}' not found in pending list")
        return False
    
    # Find and Approve Institution 2
    inst2_name = test_data.get("inst2_name")
    inst2 = next((i for i in pending_list if i["name"] == inst2_name), None)
    
    if inst2:
        inst2_id = inst2["id"]
        test_data["institution2_id"] = inst2_id
        requests.post(f"{BASE_URL}/admin/institution/{inst2_id}/approve", headers=headers)
        log_result("3.3 Kurum2 Onay", True, f"Approved Institution2 ({inst2_id})")
    else:
        log_result("3.3 Kurum2 Onay", False, f"Institution2 '{inst2_name}' not found")
        return False
        
    return True

# ============================================================================
# STEP 4: Owner 1 adds Owner 2 as Co-Owner
# ============================================================================
def step4_add_coowner():
    print("\n" + "="*50)
    print("ADIM 4: Sahip1 Sahip2'yi Co-Owner Ekle")
    print("="*50)
    
    # 4.1 Owner 1 Login
    response, error = api_call("POST", "auth/login", {
        "username": test_data["owner1_email"],
        "password": "Owner123!"
    })
    if error or response.status_code != 200:
        log_result("4.1 Sahip1 Login", False, f"Failed: {error or response.text}")
        return False
    
    test_data["owner1_token"] = response.json().get("data", {}).get("token")
    log_result("4.1 Sahip1 Login", True, "Token received")
    
    # 4.2 Add Owner 2 as co-owner of Institution 1
    response, error = api_call("POST", f"institution/{test_data['institution1_id']}/add-owner",
                               {"userId": test_data["owner2_id"]},
                               test_data["owner1_token"])
    if error or response.status_code != 200:
        log_result("4.2 Co-Owner Ekleme", False, f"Failed: {error or response.text}")
        return False
    
    log_result("4.2 Co-Owner Ekleme", True, f"Owner2 ({test_data['owner2_id']}) added to Institution1")
    
    test_data["owner1_token"] = None
    log_result("4.3 Sahip1 Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 5: Owner 2 checks institutions
# ============================================================================
def step5_check_institutions():
    print("\n" + "="*50)
    print("ADIM 5: Sahip2 Kurumları Kontrol")
    print("="*50)
    
    # 5.1 Owner 2 Login
    response, error = api_call("POST", "auth/login", {
        "username": test_data["owner2_email"],
        "password": "Owner123!"
    })
    if error or response.status_code != 200:
        log_result("5.1 Sahip2 Login", False, f"Failed: {error or response.text}")
        return False
    
    test_data["owner2_token"] = response.json().get("data", {}).get("token")
    log_result("5.1 Sahip2 Login", True, "Token received")
    
    # 5.2 Get My Institutions
    response, error = api_call("GET", "institution/my", None, test_data["owner2_token"])
    if error or response.status_code != 200:
        log_result("5.2 Kurumları Listele", False, f"Failed: {error or response.text}")
        return False
    
    try:
        data = response.json()
        institutions = data.get("data", [])
        # Owner 2 should see 2 institutions (their own + co-owner of Institution 1)
        if len(institutions) >= 2:
            log_result("5.2 Kurumları Listele", True, 
                       f"{len(institutions)} kurum bulundu (beklenen: 2)")
        else:
            log_result("5.2 Kurumları Listele", False, 
                       f"Sadece {len(institutions)} kurum bulundu (beklenen: 2)", 
                       {"institutions": institutions})
            return False
    except Exception as e:
        log_result("5.2 Kurumları Listele", False, f"Parse error: {e}")
        return False
    
    # Keep Owner 2 logged in for step 6
    return True

# ============================================================================
# STEP 6: Owner 2 creates 2 Managers in Institution 1
# ============================================================================
def step6_create_managers():
    print("\n" + "="*50)
    print("ADIM 6: Sahip2 Yönetici Oluşturma")
    print("="*50)
    
    # Owner 2 should still be logged in from step 5
    if not test_data["owner2_token"]:
        # Re-login if needed
        response, error = api_call("POST", "auth/login", {
            "username": test_data["owner2_email"],
            "password": "Owner123!"
        })
        if error or response.status_code != 200:
            log_result("6.0 Sahip2 Re-Login", False, f"Failed: {error or response.text}")
            return False
        test_data["owner2_token"] = response.json().get("data", {}).get("token")
    
    timestamp = int(time.time())
    
    # 6.1 Create Manager 1
    manager1_data = {
        "fullName": "Yönetici 1",
        "email": f"manager1_{timestamp}@test.com",
        "password": "Manager123!",
        "phone": "5553333333"
    }
    
    response, error = api_call("POST", f"institution/{test_data['institution1_id']}/managers",
                               manager1_data, test_data["owner2_token"])
    if error or response.status_code != 200:
        log_result("6.1 Yönetici1 Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    test_data["manager1_id"] = response.json().get("data")
    test_data["manager1_email"] = manager1_data["email"]
    log_result("6.1 Yönetici1 Oluştur", True, f"ManagerId: {test_data['manager1_id']}")
    
    # 6.2 Create Manager 2
    manager2_data = {
        "fullName": "Yönetici 2",
        "email": f"manager2_{timestamp}@test.com",
        "password": "Manager123!",
        "phone": "5554444444"
    }
    
    response, error = api_call("POST", f"institution/{test_data['institution1_id']}/managers",
                               manager2_data, test_data["owner2_token"])
    if error or response.status_code != 200:
        log_result("6.2 Yönetici2 Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    test_data["manager2_id"] = response.json().get("data")
    log_result("6.2 Yönetici2 Oluştur", True, f"ManagerId: {test_data['manager2_id']}")
    
    test_data["owner2_token"] = None
    log_result("6.3 Sahip2 Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 7: Manager 1 creates teachers, classrooms, students
# ============================================================================
def step7_manager_operations():
    print("\n" + "="*50)
    print("ADIM 7: Yönetici1 Öğretmen/Sınıf/Öğrenci Oluşturma")
    print("="*50)
    
    timestamp = int(time.time())
    
    # 7.1 Manager 1 Login
    response, error = api_call("POST", "auth/login", {
        "username": test_data["manager1_email"],
        "password": "Manager123!"
    })
    if error or response.status_code != 200:
        log_result("7.1 Yönetici1 Login", False, f"Failed: {error or response.text}")
        return False
    
    test_data["manager1_token"] = response.json().get("data", {}).get("token")
    log_result("7.1 Yönetici1 Login", True, "Token received")
    
    # 7.2 Create 5 Teachers
    for i in range(1, 6):
        teacher_data = {
            "fullName": f"Öğretmen {i}",
            "email": f"teacher{i}_{timestamp}@test.com",
            "password": "Teacher123!",
            "phone": f"555600000{i}",
            "employeeNumber": f"TCH-{timestamp}-{i:03d}"
        }
        
        response, error = api_call("POST", f"institution/{test_data['institution1_id']}/create-teacher",
                                   teacher_data, test_data["manager1_token"])
        if error or response.status_code != 200:
            log_result(f"7.2.{i} Öğretmen{i} Oluştur", False, f"Failed: {error or response.text}")
            continue
        
        teacher_id = response.json().get("data")
        test_data["teacher_ids"].append(teacher_id)
        test_data["teacher_tokens"].append({
            "email": teacher_data["email"],
            "password": "Teacher123!",
            "id": teacher_id
        })
        log_result(f"7.2.{i} Öğretmen{i} Oluştur", True, f"TeacherId: {teacher_id}")
    
    # 7.3 Create 2 Classrooms
    classroom1_data = {
        "institutionId": test_data["institution1_id"],
        "name": f"11-A Sınıfı",
        "grade": 11
    }
    
    response, error = api_call("POST", "classroom/create", classroom1_data, test_data["manager1_token"])
    if error or response.status_code != 200:
        log_result("7.3.1 Sınıf1 Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    test_data["classroom1_id"] = response.json().get("data")
    log_result("7.3.1 Sınıf1 Oluştur", True, f"ClassroomId: {test_data['classroom1_id']}")
    
    classroom2_data = {
        "institutionId": test_data["institution1_id"],
        "name": f"11-B Sınıfı",
        "grade": 11
    }
    
    response, error = api_call("POST", "classroom/create", classroom2_data, test_data["manager1_token"])
    if error or response.status_code != 200:
        log_result("7.3.2 Sınıf2 Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    test_data["classroom2_id"] = response.json().get("data")
    log_result("7.3.2 Sınıf2 Oluştur", True, f"ClassroomId: {test_data['classroom2_id']}")
    
    # 7.4 Create 10 Students
    for i in range(1, 11):
        student_data = {
            "fullName": f"Öğrenci {i}",
            "email": f"student{i}_{timestamp}@test.com",
            "password": "Student123!",
            "phone": f"555700000{i}",
            "studentNumber": f"STU{i:03d}"
        }
        
        response, error = api_call("POST", f"institution/{test_data['institution1_id']}/create-student",
                                   student_data, test_data["manager1_token"])
        if error or response.status_code != 200:
            log_result(f"7.4.{i} Öğrenci{i} Oluştur", False, f"Failed: {error or response.text}")
            continue
        
        student_id = response.json().get("data")
        test_data["student_ids"].append({
            "id": student_id,
            "email": student_data["email"],
            "studentNumber": student_data["studentNumber"].strip()
        })
        log_result(f"7.4.{i} Öğrenci{i} Oluştur", True, f"StudentId: {student_id}")
    
    # 7.5 Get institution user IDs for classroom assignment
    # First get all members to get InstitutionUser IDs
    response, error = api_call("GET", f"institution/{test_data['institution1_id']}/members",
                               {"role": 3}, test_data["manager1_token"])  # Student role = 3
    
    if error or response.status_code != 200:
        log_result("7.5 Öğrenci Listesi", False, f"Failed: {error or response.text}")
        return False
    
    institution_students = response.json().get("data", {}).get("items", [])
    log_result("7.5 Öğrenci Listesi", True, f"{len(institution_students)} öğrenci bulundu")
    
    # 7.6 Assign 5 students to Classroom 1
    if len(institution_students) >= 5:
        student_ids_classroom1 = [s["id"] for s in institution_students[:5]]
        response, error = api_call("POST", "classroom/add-students-bulk",
                                   {"classroomId": test_data['classroom1_id'], "studentIds": student_ids_classroom1},
                                   test_data["manager1_token"])
        if error or response.status_code != 200:
            log_result("7.6 Sınıf1 Öğrenci Atama", False, f"Failed: {error or response.text}")
        else:
            log_result("7.6 Sınıf1 Öğrenci Atama", True, f"5 öğrenci atandı")
    
    # 7.7 Assign 5 students to Classroom 2
    if len(institution_students) >= 10:
        student_ids_classroom2 = [s["id"] for s in institution_students[5:10]]
        response, error = api_call("POST", "classroom/add-students-bulk",
                                   {"classroomId": test_data['classroom2_id'], "studentIds": student_ids_classroom2},
                                   test_data["manager1_token"])
        if error or response.status_code != 200:
            log_result("7.7 Sınıf2 Öğrenci Atama", False, f"Failed: {error or response.text}")
        else:
            log_result("7.7 Sınıf2 Öğrenci Atama", True, f"5 öğrenci atandı")
    
    test_data["manager1_token"] = None
    log_result("7.8 Yönetici1 Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 8: Teacher creates exam, processes optical results
# ============================================================================
def step8_teacher_exam():
    print("\n" + "="*50)
    print("ADIM 8: Öğretmen Sınav ve Karne İşlemleri")
    print("="*50)
    
    if not test_data["teacher_tokens"]:
        log_result("8.0 Öğretmen Yok", False, "No teachers created")
        return False
    
    # 8.1 Teacher Login
    teacher = test_data["teacher_tokens"][0]
    response, error = api_call("POST", "auth/login", {
        "username": teacher["email"],
        "password": teacher["password"]
    })
    if error or response.status_code != 200:
        log_result("8.1 Öğretmen Login", False, f"Failed: {error or response.text}")
        return False
    
    teacher_token = response.json().get("data", {}).get("token")
    log_result("8.1 Öğretmen Login", True, "Token received")
    
    # 8.2 Create Exam (5 questions)
    # Answer key: A, B, C, D, E
    # Lesson config: 
    # - Matematik (0-1): 2 sorular, Topic: "Temel İşlemler"
    # - Türkçe (2-3): 2 sorular, Topic: "Paragraf"  
    # - Fen Bilgisi (4): 1 soru, Topic: "Canlılar"
    
    exam_data = {
        "institutionId": test_data["institution1_id"],
        "classroomId": test_data["classroom1_id"],
        "title": "Deneme Sınavı 1",
        "type": 0,  # DenemeExam
        "examDate": datetime.now().isoformat(),
        "answerKeyJson": json.dumps({
            "Matematik": "AB",
            "Türkçe": "CD",
            "FenBilgisi": "E"
        }),
        "lessonConfigJson": json.dumps({
            "Matematik": {
                "StartIndex": 0,
                "QuestionCount": 2,
                "TopicMapping": {"0": "Temel İşlemler", "1": "Temel İşlemler"}
            },
            "Türkçe": {
                "StartIndex": 2,
                "QuestionCount": 2,
                "TopicMapping": {"0": "Paragraf", "1": "Paragraf"}
            },
            "FenBilgisi": {
                "StartIndex": 4,
                "QuestionCount": 1,
                "TopicMapping": {"0": "Canlılar"}
            }
        })
    }
    
    response, error = api_call("POST", "exam/create", exam_data, teacher_token)
    if error or response.status_code != 200:
        log_result("8.2 Sınav Oluştur", False, f"Failed: {error or response.text}")
        return False
    
    test_data["exam_id"] = response.json().get("data")
    log_result("8.2 Sınav Oluştur", True, f"ExamId: {test_data['exam_id']}")
    
    # 8.3 Create Optical Files for 5 students
    os.makedirs(OPTICAL_OUTPUT_DIR, exist_ok=True)
    
    # Student answers (varies per student for different results)
    student_answers = [
        "ABCDE",  # Student 1: 5/5 doğru
        "ABCDX",  # Student 2: 4/5 doğru  
        "ABXDE",  # Student 3: 3/5 doğru (Türkçe 1 yanlış)
        "AXCDE",  # Student 4: 4/5 doğru (Mat 1 yanlış)
        "XBCDE",  # Student 5: 4/5 doğru (Mat 1 yanlış)
    ]
    
    # Create optical file content
    # Format: [ÖğrenciNo(10)][AdSoyad(25)][Kitapçık(1)][Cevaplar]
    optical_content = ""
    for i, student in enumerate(test_data["student_ids"][:5]):
        student_no = f"STU{i+1:03d}".ljust(10) 
        student_name = f"Öğrenci {i+1}".ljust(25)
        booklet = "A"
        answers = student_answers[i] if i < len(student_answers) else "ABCDE"
        optical_content += f"{student_no}{student_name}{booklet}{answers}\n"
    
    optical_file_path = os.path.join(OPTICAL_OUTPUT_DIR, f"optical_exam_{test_data['exam_id']}.txt")
    with open(optical_file_path, "w", encoding="utf-8") as f:
        f.write(optical_content)
    
    log_result("8.3 Optik Dosya Oluştur", True, f"File: {optical_file_path}")
    
    # 8.4 Process Optical Results
    with open(optical_file_path, "rb") as f:
        files = {"file": ("optical.txt", f, "text/plain")}
        response, error = api_call("POST", f"exam/{test_data['exam_id']}/process-optical",
                                   None, teacher_token, files=files)
    
    if error or response.status_code != 200:
        log_result("8.4 Optik Sonuç İşle", False, f"Failed: {error or response.text}")
        return False
    
    log_result("8.4 Optik Sonuç İşle", True, "Results processed")
    
    # 8.5 Confirm Results
    # 8.5 Confirm Results
    response, error = api_call("POST", f"exam/{test_data['exam_id']}/confirm",
                               None, teacher_token)
    if error or response.status_code != 200:
        log_result("8.5 Sonuçları Onayla", False, f"Failed: {error or response.text}")
        return False
    
    log_result("8.5 Sonuçları Onayla", True, "Results confirmed and ranking calculated")
    
    # 8.6 Get Exam Results
    response, error = api_call("GET", f"exam/{test_data['exam_id']}/results",
                               None, teacher_token)
    if error or response.status_code != 200:
        log_result("8.6 Sonuçları Getir", False, f"Failed: {error or response.text}")
    else:
        exam_results = response.json().get("data", [])
        test_data["exam_results"] = exam_results
        log_result("8.6 Sonuçları Getir", True, f"{len(exam_results)} sonuç bulundu")
        
        # Save results to file
        results_file = os.path.join(OPTICAL_OUTPUT_DIR, f"results_exam_{test_data['exam_id']}.json")
        with open(results_file, "w", encoding="utf-8") as f:
            json.dump(exam_results, f, ensure_ascii=False, indent=2)
        log_result("8.7 Sonuçları Kaydet", True, f"File: {results_file}")
    
    # 8.8 Send results to classroom (using Message endpoint)
    if test_data["exam_results"]:
        result_ids = [r.get("id") for r in test_data["exam_results"] if r.get("id")]
        response, error = api_call("POST", "message/send-to-class",
                                   {"classroomId": test_data["classroom1_id"], "reportCardIds": result_ids},
                                   teacher_token)
        if error or response.status_code != 200:
            log_result("8.8 Sınıfa Karne Gönder", False, f"Failed: {error or response.text}")
        else:
            log_result("8.8 Sınıfa Karne Gönder", True, f"{len(result_ids)} karne gönderildi")
    
    log_result("8.9 Öğretmen Logout", True, "Token cleared")
    
    return True

# ============================================================================
# STEP 9: Student checks report card, sends message
# ============================================================================
def step9_student_verification():
    print("\n" + "="*50)
    print("ADIM 9: Öğrenci Karne Kontrolü ve Mesaj")
    print("="*50)
    
    if not test_data["student_ids"]:
        log_result("9.0 Öğrenci Yok", False, "No students created")
        return False
    
    # 9.1 Student Login
    student = test_data["student_ids"][0]
    response, error = api_call("POST", "auth/login", {
        "username": student["email"],
        "password": "Student123!"
    })
    if error or response.status_code != 200:
        log_result("9.1 Öğrenci Login", False, f"Failed: {error or response.text}")
        return False
    
    student_token = response.json().get("data", {}).get("token")
    log_result("9.1 Öğrenci Login", True, "Token received")
    
    # 9.2 Get My Reports
    response, error = api_call("GET", f"report/student/{student['id']}/all", None, student_token)
    if error or response.status_code != 200:
        log_result("9.2 Karneleri Getir", False, f"Failed: {error or response.text}")
    else:
        reports = response.json().get("data", [])
        log_result("9.2 Karneleri Getir", True, f"{len(reports)} karne bulundu")
        
        # Verify report details
        if reports:
            report = reports[0]
            details = {
                "totalCorrect": report.get("totalCorrect"),
                "totalWrong": report.get("totalWrong"),
                "totalNet": report.get("totalNet"),
                "classRank": report.get("classRank"),
                "lessons": [l.get("lessonName") for l in report.get("lessons", [])]
            }
            log_result("9.3 Karne Detayları", True, "", details)
    
    # 9.4 Get Conversations (find class group)
    response, error = api_call("GET", "message/conversations", None, student_token)
    if error or response.status_code != 200:
        log_result("9.4 Konuşmaları Getir", False, f"Failed: {error or response.text}")
        return False
    
    conversations = response.json().get("data", [])
    log_result("9.4 Konuşmaları Getir", True, f"{len(conversations)} konuşma bulundu")
    
    # 9.5 Send message to class group
    if conversations:
        class_conv = next((c for c in conversations if c.get("isGroup")), conversations[0])
        
        message_data = {
            "conversationId": class_conv.get("id"),
            "content": "Merhaba, karne sonuçlarımı gördüm. Teşekkürler!",
            "type": 0
        }
        
        response, error = api_call("POST", "message/send", message_data, student_token)
        if error or response.status_code != 200:
            log_result("9.5 Sınıf Grubuna Mesaj", False, f"Failed: {error or response.text}")
        else:
            log_result("9.5 Sınıf Grubuna Mesaj", True, "Mesaj gönderildi")
    
    log_result("9.6 Öğrenci Logout", True, "Token cleared")
    
    return True


# ============================================================================
# MAIN
# ============================================================================
def main():
    print("="*60)
    print("KAPSAMLI SENARYO TESTİ BAŞLIYOR")
    print("="*60)
    print(f"API Base URL: {BASE_URL}")
    print(f"Tarih: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("="*60)
    
    try:
        # Run all steps
        step1_admin_operations()
        step2_owner_registration()
        step3_admin_approvals()
        step4_add_coowner()
        step5_check_institutions()
        step6_create_managers()
        step7_manager_operations()
        step8_teacher_exam()
        step9_student_verification()
        
    except Exception as e:
        log_result("CRITICAL ERROR", False, str(e))
    
    finally:
        save_report()
        
        # Summary
        passed = sum(1 for r in results if r["status"] == "SUCCESS")
        failed = sum(1 for r in results if r["status"] == "FAIL")
        
        print("\n" + "="*60)
        print(f"TEST TAMAMLANDI: {passed} Başarılı / {failed} Başarısız")
        print("="*60)


if __name__ == "__main__":
    main()
