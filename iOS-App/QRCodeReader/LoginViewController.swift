//
//  AddDeviceViewController.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-18.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import UIKit
import Alamofire
import SwiftKeychainWrapper

class LoginViewController: UIViewController {
    
    //Outlets
    @IBOutlet weak var usernameField: UITextField!
    @IBOutlet weak var passwordField: UITextField!
    @IBOutlet weak var loginButton: UIButton!

    
    override func viewDidLoad() {
        super.viewDidLoad()
        setUpTextFields()
        
    }
    
    override func viewDidAppear(_ animated: Bool) {
        checkToken();
    }
    
    func setUpTextFields(){
        self.usernameField.attributedPlaceholder = NSAttributedString(string: "Username",attributes: [NSAttributedString.Key.foregroundColor: UIColor.lightGray])
        self.passwordField.attributedPlaceholder = NSAttributedString(string: "Password",attributes: [NSAttributedString.Key.foregroundColor: UIColor.lightGray])
        loginButton.layer.cornerRadius =  10
    }
    
    func checkToken(){
        
        if let _ = KeychainWrapper.standard.string(forKey: "TOKEN"), let expirationTimeStamp = KeychainWrapper.standard.double(forKey: "EXPIRATION_TIMESTAMP"){
            print("Token was found");
            if(expirationTimeStamp > Date().timeIntervalSince1970){
                print("Token valid");
                performSegue(withIdentifier: "loginInSegueWithoutAnimation", sender: nil)
                return;
            }
        }
        print("Token not found or expired");
    }

    
    @IBAction func fieldEditingEnded(_ sender: Any) {
        loginButton.isEnabled = usernameField.text != "" && passwordField.text != ""
    }
    
    @IBAction func loginButtonPressed(_ sender: Any) {
        let username = self.usernameField.text!;
        let password = self.passwordField.text!;
        
        let loginURL = URL(string: "\(thingsboard_url)/api/auth/login")!
        let body = [
            "username": username,
            "password": password
        ]
        let headers: HTTPHeaders = [
            "Content-Type":"application/json"
        ]
        
        Alamofire.request(loginURL, method: .post, parameters: body, encoding: JSONEncoding.default, headers: headers).responseJSON{ response in
            guard let value = response.result.value, let json = value as? [String:Any] else {
                print("Could not convert login response to JSON");
                return;
            }
            guard let token = json["token"] as? String else{
                print("Couldn't get token");
                return;
            }
            X_Authorization = "Bearer \(token)"
            let wasTokenSet: Bool = KeychainWrapper.standard.set(token, forKey: "TOKEN");
            print("Token saved successfulyy: \(wasTokenSet)");
            let expirationTime = Date().addingTimeInterval(EXPIRATION_DURATION).timeIntervalSince1970
            let _ = KeychainWrapper.standard.set(expirationTime, forKey: "EXPIRATION_TIMESTAMP")
            self.performSegue(withIdentifier: "loginSegue", sender: nil);
        }
    }
    @IBAction func tappedOnScreen(_ sender: Any) {
        print("tapped")
        view.endEditing(true)
    }
    
    @IBAction func unwindToLogin(segue: UIStoryboardSegue){}
    
}


