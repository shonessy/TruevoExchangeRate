from datetime import datetime
from msilib.schema import Error
import unittest
import conversion_constants as cc
from models import HeaderModel, DetailRecordModel, TrailerModel, ResponseModel
import main

class TestConversion(unittest.TestCase):
    
    def test_file_exists_and_not_empty_should_return_false_on_non_existing_file(self):
        temp = cc.CURRENCY_RATES_FILE_PATH
        cc.CURRENCY_RATES_FILE_PATH = "test"
        self.assertEqual(main.file_exists_and_not_empty(), False)
        cc.CURRENCY_RATES_FILE_PATH = temp

    def test_file_exists_and_not_empty_should_return_false_on_empty_file(self):
        temp = cc.CURRENCY_RATES_FILE_PATH
        cc.CURRENCY_RATES_FILE_PATH = "test.txt"
        with open(cc.CURRENCY_RATES_FILE_PATH, 'w') as file:
            pass
        self.assertEqual(main.file_exists_and_not_empty(), False)
        cc.CURRENCY_RATES_FILE_PATH = temp

    def test_file_exists_and_not_empty_should_return_true(self):
        self.assertEqual(main.file_exists_and_not_empty(), True)

    # def test_is_file_format_valid_should_return_false_on_none_headers():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_many_headers():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_none_trailers():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_many_trailers():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_header_not_being_first_line():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_trailer_not_being_last_line():
    #     pass

    # def test_is_file_format_valid_should_return_false_on_wrong_detail_record_identifier():
    #     pass

    # def test_is_file_format_valid_should_return_true_on_correct_detail_record():
    #     pass

    def test_is_data_record_allowed_should_return_false_on_none_data_record(self):
        self.assertEqual(main.is_data_record_allowed(None), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_reference_currency_code(self):
        data = DetailRecordModel(978, 978, None, None, None, None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_rate_class(self):
        data = DetailRecordModel(978, cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, "F", None, None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_rate_rate_format_indicator(self):
        data = DetailRecordModel(978, cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, "M", "P", None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)      

    def test_is_data_record_allowed_should_return_false_on_CNH(self):
        data = DetailRecordModel(cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[0], cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, cc.ALLOWED_RATE_CLASSES[0], cc.ALLOWED_RATE_FORMAT_INDICATORS[0], None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)  

    def test_is_data_record_allowed_should_return_false_on_CNY(self):
        data = DetailRecordModel(978, cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[1], cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], cc.ALLOWED_RATE_CLASSES[0], cc.ALLOWED_RATE_FORMAT_INDICATORS[0], None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)    

    def test_is_data_record_allowed_should_return_false_on_SVC(self):
        data = DetailRecordModel(cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[2], cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, cc.ALLOWED_RATE_CLASSES[0], cc.ALLOWED_RATE_FORMAT_INDICATORS[0], None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False)  
        
    def test_is_data_record_allowed_should_return_false_on_ESA(self):
        data = DetailRecordModel(cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[3], cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, cc.ALLOWED_RATE_CLASSES[0], cc.ALLOWED_RATE_FORMAT_INDICATORS[0], None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), False) 

    def test_is_data_record_allowed_should_return_true_on_correct_data_record(self):
        data = DetailRecordModel(978, cc.ALLOWED_REFERENCE_CURRENCY_CODES[0], None, cc.ALLOWED_RATE_CLASSES[0], cc.ALLOWED_RATE_FORMAT_INDICATORS[0], None, None, None)
        self.assertEqual(main.is_data_record_allowed(data), True)  

    def test_convert_file_line_to_header_should_return_none_on_none_line(self):
        self.assertEqual(main.convert_file_line_to_header(None), None) 

    def test_convert_file_line_to_header_should_return_none_on_improper_line_length(self):
        self.assertEqual(main.convert_file_line_to_header("test"), None)

    def test_convert_file_line_to_header_return_none_on_unsupported_header_format(self):
        self.assertEqual(main.convert_file_line_to_header("test"*10), None)            

    def test_convert_file_line_to_header_should_return_none_on_improper_date_format(self):
        self.assertEqual(main.convert_file_line_to_header("H15321a15645641fdsa15641fads16546"), None) 

    def test_convert_file_line_to_header_should_return_header(self):
        line = "H201710211400191                                                                                                             "
        data = main.convert_file_line_to_header(line)
        date = datetime(2017, 10, 21, 14, 0, 19)
        self.assertEqual(data.date, date) 

    def test_get_rate_should_raises_exception_on_improper_conversion_rate_length(self):
        with self.assertRaises(Exception):
            main.get_rate(None)

    def test_get_rate_should_raises_exception_on_improper_conversion_rate_length(self):
        with self.assertRaises(Exception):
            main.get_rate("123")

    def test_get_rate_should_raises_exception_on_improper_conversion_rate(self):
        with self.assertRaises(ValueError):
            main.get_rate("t"*(cc.CURRENCY_COVERSION_INTEGER_PLACES + cc.CURRENCY_COVERSION_DECIMAL_PLACES))

    def test_convert_file_line_to_trailer_should_return_none_on_none_line(self):
        self.assertEqual(main.convert_file_line_to_trailer(None), None)            

    def test_convert_file_line_to_trailer_should_return_none_on_improper_line_length(self):
        self.assertEqual(main.convert_file_line_to_trailer("test"), None)            

    def test_convert_file_line_to_trailer_should_return_none_on_improper_line_format(self):
        self.assertEqual(main.convert_file_line_to_header("test"*10), None)            

    def test_convert_file_line_to_trailer_should_return_trailer(self):
        line = "T00022200001731625193984                                                                                                     "
        data = main.convert_file_line_to_trailer(line)
        self.assertEqual(data.total_records, 222)
        self.assertEqual(data.hash_total, 1731625193984)

    def test_convert_file_line_to_detail_record_should_return_none_on_none_line(self):
        self.assertEqual(main.convert_file_line_to_detail_record(None), None)            

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_line_length(self):
        self.assertEqual(main.convert_file_line_to_detail_record("test"), None)   

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_source_currency_code(self):
        line = "Dana8402MD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)   

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_reference_currency_code(self):
        line = "D555ana2MD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)   

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_source_currency_exponent(self):
        line = "D555777pMD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)  

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_buy_currency_conversion_rate(self):
        line = "D5557772MD0000136p4316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)  

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_mid_currency_conversion_rate(self):
        line = "D5557772MD0000136243162500000136p5000000000013675683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)  

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_sell_currency_conversion_rate(self):
        line = "D5557772MD0000136543162500000136750000000000136p5683750999999999999999                                                       "
        self.assertEqual(main.convert_file_line_to_detail_record(line), None)          

    def test_convert_file_line_to_detail_record_should_return_detail_record(self):
        line = "D5557772MD000013654316250000013675000000000013675683750999999999999999                                                       "
        data = main.convert_file_line_to_detail_record(line)
        self.assertEqual(data.source_currency_code, 555)   
        self.assertEqual(data.reference_currency_code, 777)   
        self.assertEqual(data.source_currency_exponent, 2)   
        self.assertEqual(data.rate_class, "M")   
        self.assertEqual(data.rate_format_indicator, "D")   
        self.assertEqual(data.buy_currency_conversion_rate, 136.5431625)   
        self.assertEqual(data.mid_currency_conversion_rate, 136.75)   
        self.assertEqual(data.sell_currency_conversion_rate, 136.7568375)   



if __name__ == '__main__':
    unittest.main()