//
//  AttributeTableViewCell.swift
//  QRCodeReader
//
//  Created by Ayman Hashisho on 2019-03-20.
//  Copyright © 2019 AppCoda. All rights reserved.
//

import UIKit

class AttributeTableViewCell: UITableViewCell {
    
    
    @IBOutlet weak var keyLabel: UILabel!
    @IBOutlet weak var valueLabel: UILabel!
    @IBOutlet weak var dateLabel: UILabel!
    

    override func awakeFromNib() {
        super.awakeFromNib()
        // Initialization code
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)

        // Configure the view for the selected state
    }

}
