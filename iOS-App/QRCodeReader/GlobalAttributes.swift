//
//  GlobalAttributes.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-17.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import Foundation
let NOTIFICATION_SERVER_URL = URL(string: "http://notification-server.us-east-2.elasticbeanstalk.com/")!
let thingsboard_url = "http://ec2-18-217-252-69.us-east-2.compute.amazonaws.com"
let tenant_id = "a7e54bc0-f4aa-11e8-9d62-3da15cd42bab"
let api_url = "https://kwgr1v28nb.execute-api.us-east-2.amazonaws.com/dev"
var X_Authorization = "Bearer eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJ0ZW5hbnRAdGhpbmdzYm9hcmQub3JnIiwic2NvcGVzIjpbIlRFTkFOVF9BRE1JTiJdLCJ1c2VySWQiOiJhODE3MzEzMC1mNGFhLTExZTgtOWQ2Mi0zZGExNWNkNDJiYWIiLCJlbmFibGVkIjp0cnVlLCJpc1B1YmxpYyI6ZmFsc2UsInRlbmFudElkIjoiYTdlNTRiYzAtZjRhYS0xMWU4LTlkNjItM2RhMTVjZDQyYmFiIiwiY3VzdG9tZXJJZCI6IjEzODE0MDAwLTFkZDItMTFiMi04MDgwLTgwODA4MDgwODA4MCIsImlzcyI6InRoaW5nc2JvYXJkLmlvIiwiaWF0IjoxNTU0MTM2MDk2LCJleHAiOjE1NTQ3NDA4OTZ9.GYndAnf53DF76LeT5D6LPWgj8qag-TvcgPWjLX1fcT6XpZkmQ_WuvtIGlvVPdcUPNsNim7sJunhjvCmv_XV4Qg"
let EXPIRATION_DURATION = 3600.0 * 24.0 * 7.0 // One week
