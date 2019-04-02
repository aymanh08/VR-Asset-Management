//
//  Attribute.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-20.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import Foundation

class Attribute {
    
    static var dateFormatter = DateFormatter()
    static let calendar = Calendar.current
    
    let key: String
    let value: Any
    let lastUpdate: Date
    var lastUpdateString: String{
        Attribute.dateFormatter.dateFormat = "MM/dd HH:mm"
        return Attribute.dateFormatter.string(from: lastUpdate)
    }
    init(key: String, value: Any, lastUpdate: TimeInterval = Date().timeIntervalSince1970) {
        
        self.key = key;
        self.value = value;
        self.lastUpdate = Date(timeIntervalSince1970: lastUpdate/1000)
    }
}
