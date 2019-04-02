//
//  LogOutViewController.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-04-01.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import UIKit
import SwiftKeychainWrapper

class LogOutViewController: UIViewController {

    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view.
        
    }
    override func viewDidAppear(_ animated: Bool) {
        let didRemoveToken = KeychainWrapper.standard.remove(key: "TOKEN")
        let didRemoveExpiry = KeychainWrapper.standard.remove(key: "EXPIRATION_DURATION")
        print("Removed token: \(didRemoveToken)");
        print("Removed Expiry: \(didRemoveExpiry)")
        performSegue(withIdentifier: "logoutSegue", sender: nil)
    }

}
