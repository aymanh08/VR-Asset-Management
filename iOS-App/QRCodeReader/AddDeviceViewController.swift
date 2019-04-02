//
//  AddDeviceViewController.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-18.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import UIKit
import Alamofire

class AddDeviceViewController: UIViewController {
    
    //Outlets
    @IBOutlet weak var deviceNameField: UITextField!
    @IBOutlet weak var deviceTypeField: UITextField!
    @IBOutlet weak var addDeviceButton: UIButton!
    @IBOutlet weak var roomPicker: UIPickerView!
    @IBOutlet weak var roomButton: UIButton!
    
    //Rooms
    //var rooms = [Room]()
    var rooms = [Room]()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        self.deviceNameField.attributedPlaceholder = NSAttributedString(string: "Device Name",attributes: [NSAttributedString.Key.foregroundColor: UIColor.lightGray])
        self.deviceTypeField.attributedPlaceholder = NSAttributedString(string: "Device Type",attributes: [NSAttributedString.Key.foregroundColor: UIColor.lightGray])
        getRooms()
        addDeviceButton.layer.cornerRadius =  10
//        deviceTypeField.layer.borderWidth = 0.5
//        deviceNameField.layer.borderWidth = 0.5
//        deviceTypeField.layer.cornerRadius = 10
//        deviceNameField.layer.cornerRadius = 10
//
//        deviceTypeField.layer.borderColor = #colorLiteral(red: 0.3209896088, green: 0.8729352355, blue: 0.9214736819, alpha: 1)
//        deviceNameField.layer.borderColor = #colorLiteral(red: 0.3209896088, green: 0.8729352355, blue: 0.9214736819, alpha: 1)
        
    }
    
    func addDevice(deviceName: String, deviceType: String, room: Room){
        
        let devicesURL = URL(string: thingsboard_url + "/api/device")!
        let body: Parameters = [
            "name": deviceName,
            "tenantId": [
                "entityType": "TENANT",
                "id": tenant_id
            ],
            "type": deviceType
        ]
        let headers: HTTPHeaders = [
            "X-Authorization": X_Authorization,
            "Content-Type": "application/json"
        ]
        Alamofire.request(devicesURL, method: .post, parameters: body, encoding: JSONEncoding.default, headers: headers).responseJSON{ response in
            
            guard let value = response.result.value, let json = value as? [String:Any] else {
                let alert = UIAlertController(title: "Error", message: "Unknown error occured", preferredStyle: .alert)
                let cancelAction = UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.cancel, handler: nil)
                alert.addAction(cancelAction)
                self.present(alert, animated: true, completion: nil)
                return
            }
            
            
            
            if(response.response?.statusCode == 200){
                guard let idSection = json["id"] as? [String:String], let id = idSection["id"] else{
                    return
                }
                self.assignToRoom(room: room, deviceId: id)
                let alert = UIAlertController(title: "Success", message: "Device succesfully addded", preferredStyle: .alert)
                let cancelAction = UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.cancel, handler: nil)
                alert.addAction(cancelAction)
                self.present(alert, animated: true, completion: nil)
            }else{
                guard let errorMessage = json["message"] as? String else{
                    let alert = UIAlertController(title: "Error", message: "Unknown error occured", preferredStyle: .alert)
                    alert.addAction(UIAlertAction(title: "Dismiss", style: .default, handler: { action in
                        self.dismiss(animated: true, completion: nil)
                    }))
                    self.present(alert, animated: true, completion: nil)
                    return
                }
                let alert = UIAlertController(title: "Error", message: errorMessage, preferredStyle: .alert)
                let cancelAction = UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.cancel, handler: nil)
                alert.addAction(cancelAction)
                self.present(alert, animated: true, completion: nil)
                return
            }
            
            
        }
    }
    
    func assignToRoom(room: Room, deviceId: String){
        let addRoom_url = URL(string: "\(thingsboard_url)/api/relation")!
        let parameters: Parameters = [
            "from":[
                "entityType":"ASSET",
                "id": room.id
            ],
            "to":[
                "entityType":"DEVICE",
                "id": deviceId
            ],
            "type": "Contains"
        ]
        let headers: HTTPHeaders = ["X-Authorization": X_Authorization]
        
        Alamofire.request(addRoom_url, method: .post, parameters: parameters, encoding: JSONEncoding.default, headers: headers).responseJSON{ response in
            guard let value = response.result.value, let json = value as? [String:Any] else {
                print("Could not convert to JSON")
                return
            }
            print(json)
            print("added relation")
            
        }
    }
    func getRooms(){
        let rooms_url = URL(string: "\(api_url)/rooms")!
        let token = X_Authorization.split(separator: " ")[1]
        let headers: HTTPHeaders = ["token":String(token)]
        
        Alamofire.request(rooms_url, encoding: URLEncoding.default, headers: headers).responseJSON{ response in
            guard let value = response.result.value, let json = value as? [[String:String]] else {
                print("Could not parse JSON");
                return;
            }
            for room in json{
                guard let name = room["name"], let id = room["id"] else{
                    return
                }
                self.rooms.append(Room(name: name, id: id))
            }
            self.roomPicker.reloadAllComponents();
            
        }
    }
    
    
    @IBAction func selectRoom(_ sender: Any) {
        UIView.animate(withDuration: 0.5, delay: 0, options: .curveEaseOut, animations: {
            self.roomPicker.alpha = 1
        }, completion: nil)
    }
    
    @IBAction func fieldEditingEnded(_ sender: Any) {
        addDeviceButton.isEnabled = deviceNameField.text != "" && deviceTypeField.text != ""
    }
    
    @IBAction func addDeviceButtonPressed(_ sender: Any) {
        let roomIndex = self.roomPicker.selectedRow(inComponent: 0)
        addDevice(deviceName: deviceNameField.text!, deviceType: deviceTypeField.text!, room: self.rooms[roomIndex])
    }
    @IBAction func tappedOnScreen(_ sender: Any) {
        print("tapped")
        view.endEditing(true)
        UIView.animate(withDuration: 0.5, delay: 0, options: .curveEaseOut, animations: {
            self.roomPicker.alpha = 0
        }, completion: nil)
    }
    
}

extension AddDeviceViewController: UIPickerViewDelegate, UIPickerViewDataSource{
    func numberOfComponents(in pickerView: UIPickerView) -> Int {
        return 1
    }
    
    func pickerView(_ pickerView: UIPickerView, numberOfRowsInComponent component: Int) -> Int {
        return rooms.count
    }
    
    func pickerView(_ pickerView: UIPickerView, attributedTitleForRow row: Int, forComponent component: Int) -> NSAttributedString? {
        return NSAttributedString(string: self.rooms[row].name, attributes: [NSAttributedString.Key.foregroundColor: UIColor.white])
    }
    func pickerView(_ pickerView: UIPickerView, didSelectRow row: Int, inComponent component: Int) {
        print(rooms[row].name)
        roomButton.titleLabel?.text = rooms[row].name
    }
    
}

