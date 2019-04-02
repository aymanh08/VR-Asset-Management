//
//  GraphViewController.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-16.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import UIKit
import SocketIO
import Alamofire
import Charts

class GraphViewController: UIViewController {
    
    
    
    //Outlets
    @IBOutlet weak var backButton: UIButton!
    @IBOutlet weak var lineChartView: LineChartView!
    @IBOutlet weak var attributesTableView: UITableView!
    
    //@IBOutlet weak var attributesTableView: UITableView!
    
    //Properties
    var deviceInfo: [String:Any]!
    let manager = SocketManager(socketURL: NOTIFICATION_SERVER_URL, config: [.log(false), .compress])
    var socket: SocketIOClient!
    var attributes = [Attribute]()
    let TELEMETRY_CAPACITY = 10;
    var telemeteryData: [ChartDataEntry] = []{
        didSet {
            if(telemeteryData.count > TELEMETRY_CAPACITY){
                telemeteryData.remove(at: 0)
            }
        }
    };
    var count = 0.0;

    override func viewDidLoad(){
        
        //Set up socket
        setUpSocket();

        lineChartView.xAxis.axisLineColor = #colorLiteral(red: 0.3209896088, green: 0.8729352355, blue: 0.9214736819, alpha: 1)
        lineChartView.leftAxis.axisLineColor = #colorLiteral(red: 0.3209896088, green: 0.8729352355, blue: 0.9214736819, alpha: 1)
        lineChartView.xAxis.labelTextColor = .white
        lineChartView.leftAxis.labelTextColor = .white
        lineChartView.legend.textColor = .white
        
        lineChartView.xAxis.gridLineWidth = 0
        lineChartView.rightAxis.gridLineWidth = 0
        lineChartView.leftAxis.gridLineWidth = 0
        lineChartView.rightAxis.enabled = false 
        getDeviceIdAndAttributes();
        
        
        
    }
    
    //Methods
    func setUpSocket(){
        
        //Get default socket from manager
        socket = manager.defaultSocket
        
        //On Connect Event
        socket.on(clientEvent: .connect) {data, ack in
            print("Received from server: ", data)
            //Subscribe to chosen device
            guard let deviceName = self.deviceInfo["deviceName"] as? String else{return}
            self.socket.emit("subscribe", deviceName){
                print("Sent subscription request");
            }
        }
        
        //On New Telemetry
        socket.on("NEW_TELEMETRY") {data, ack in
            
            //Casting the data
            guard let telemetryBody = data[0] as? [String:Any] else{ return }
            guard let meta = telemetryBody["meta"] as? [String:Any] else { return }
            
            //self.populateScreen(telemetryBody: telemetryBody, metadata: meta)
            self.updateChart(telemetry: telemetryBody, metadata: meta)
        }
        
        //Connect
        socket.connect()
    }
    

    func getDeviceIdAndAttributes(){
        
        let devices_url = URL(string: thingsboard_url + "/api/tenant/devices")!
        let parameters: Parameters = ["deviceName":deviceInfo["deviceName"] as! String];
        let headers: HTTPHeaders = ["X-Authorization": X_Authorization]
        
        Alamofire.request(devices_url, method: .get, parameters: parameters, encoding: URLEncoding.default, headers: headers).responseJSON{ response in
            guard let value = response.result.value, let json = value as? [String:Any] else {
                print("Could not convert body to json")
                return
            }
            guard let id = json["id"] as? [String:String], let entityType = id["entityType"], let deviceId = id["id"] else{
                return
            }
            self.deviceInfo["entityType"] = entityType
            self.deviceInfo["deviceId"] = deviceId;
            self.getAttributes();
            
        }
    }
    
    func getAttributes(){
        let attributes_url = URL(string: "\(thingsboard_url)/api/plugins/telemetry/\(deviceInfo["entityType"]!)/\(deviceInfo["deviceId"]!)/values/attributes")!
        let headers: HTTPHeaders = ["X-Authorization": X_Authorization]
        
        Alamofire.request(attributes_url, encoding: JSONEncoding.default, headers: headers).responseJSON{ response in
            guard let value = response.result.value, let json = value as? [[String:Any]] else {
                print("Could not parse JSON body");
                return;
            }
            for attribute in json{
                guard let key = attribute["key"] as? String, let value = attribute["value"], let lastUpdate = attribute["lastUpdateTs"] as? Double else{
                    return
                }
                self.attributes.append(Attribute(key: key, value: value, lastUpdate: lastUpdate))
            }
            self.attributesTableView.reloadData();
            print("attributes are ",self.attributes);
            
        }
    }
    
    func updateChart(telemetry: [String:Any], metadata: [String:Any]) {
        
        guard let telemetryKeysString = metadata["telemetryKeys"] as? String else{
        return
        }

        let telemetryKeys = telemetryKeysString.split(separator: ",");

        let readingTitle = String(telemetryKeys[0]);

        //Setting the reading
        guard let reading = telemetry[readingTitle] as? Double, let tsString = metadata["ts"] as? String else{
            
            return
        }
        //let ts = Double(tsString)!
        telemeteryData.append(ChartDataEntry(x: count, y: reading))
        count += 1.0;
        

        let set1 = LineChartDataSet(values: telemeteryData, label: readingTitle)
        set1.mode = .cubicBezier
        
        set1.fillColor = #colorLiteral(red: 0.3209896088, green: 0.8729352355, blue: 0.9214736819, alpha: 1)
        set1.drawFilledEnabled = true;
        set1.valueTextColor = .white
        let data = LineChartData(dataSet: set1)
        self.lineChartView.data = data
        
    }
    
    func addAttributes(key: String, value: Any){
        let attributes_url = URL(string: "\(thingsboard_url)/api/plugins/telemetry/\(deviceInfo["deviceId"]!)/SERVER_SCOPE")!
        let parameters: Parameters = [key:value]
        let headers: HTTPHeaders = ["X-Authorization":X_Authorization]
        
        Alamofire.request(attributes_url, method: .post, parameters: parameters, encoding: JSONEncoding.default, headers: headers).responseJSON{ response in
            
            if(response.response?.statusCode == 200){
                self.attributes.append(Attribute(key: key, value: value))
                self.attributesTableView.reloadData();
                let alert = UIAlertController(title: "Success", message: "Attribute succesfully addded", preferredStyle: .alert)
                let cancelAction = UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.cancel, handler: nil)
                alert.addAction(cancelAction)
                self.present(alert, animated: true, completion: nil)
            }else{
                guard let value = response.result.value, let json = value as? [String:Any], let errorMessage = json["message"] as? String else {
                    let alert = UIAlertController(title: "Error", message: "Unknown error occured", preferredStyle: .alert)
                    let cancelAction = UIAlertAction(title: "Dismiss", style: UIAlertActionStyle.cancel, handler: nil)
                    alert.addAction(cancelAction)
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
    
    func deleteAttribute(key: String){
        let delete_url = URL(string: "\(thingsboard_url)/api/plugins/telemetry/\(deviceInfo["entityType"]!)/\(deviceInfo["deviceId"]!)/SERVER_SCOPE")!
        let parameters:Parameters = ["keys":key]
        let headers: HTTPHeaders = ["X-Authorization":X_Authorization]
        
        Alamofire.request(delete_url, method: .delete, parameters: parameters, encoding: URLEncoding.default, headers: headers).responseJSON{ response in
            print(response.response?.statusCode)
            
        }
    }
    
    
    //Outlet actions
    @IBAction func backButtonClicked(_ sender: Any) {
        dismiss(animated: true, completion: {
            self.socket.disconnect();
            print("Disconnected");
        })
    }
    
    @IBAction func addAttribute(_ sender: UIButton) {
        //1. Create the alert controller.
        let alert = UIAlertController(title: "Add Attribute", message: "Enter attributes values", preferredStyle: .alert)
        
        //2. Add the text field. You can configure it however you need.
        alert.addTextField { (textField) in
            textField.placeholder = "Attribute Key"
        }
        
        alert.addTextField { (textField) in
            textField.placeholder = "Attribute Value"
        }
        
        // 3. Grab the value from the text field, and print it when the user clicks OK.
        let cancelAction = UIAlertAction(title: "Cancel", style: UIAlertActionStyle.cancel, handler: nil)
        alert.addAction(cancelAction)
        
        alert.addAction(UIAlertAction(title: "Add", style: .default, handler: { [weak alert] (_) in
            let attributeKey = alert?.textFields![0].text!
            let attributeValue = alert?.textFields![1].text!
            self.addAttributes(key: attributeKey!, value: attributeValue!)
        }))
        
        
        // 4. Present the alert.
        self.present(alert, animated: true, completion: nil)
    }
    
    
    
    
}
extension GraphViewController: UITableViewDataSource, UITableViewDelegate{
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return attributes.count
    }

    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = attributesTableView.dequeueReusableCell(withIdentifier: "attributesCellPrototype") as? AttributeTableViewCell
        cell?.keyLabel.text = attributes[indexPath.row].key
        cell?.valueLabel.text = "\(attributes[indexPath.row].value)"
        cell?.dateLabel.text = attributes[indexPath.row].lastUpdateString
        return cell!;
    }
    
    func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCellEditingStyle, forRowAt indexPath: IndexPath) {
        if editingStyle == .delete {
            self.deleteAttribute(key: self.attributes[indexPath.row].key)
            self.attributes.remove(at: indexPath.row)
            self.attributesTableView.deleteRows(at: [indexPath], with: .fade)
        }
    }
    
}

