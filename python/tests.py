import os
import random
import unittest
from datetime import datetime

import conversion_constants as cc
import lambda_function
from models import DetailRecordModel


class TestConversion(unittest.TestCase):
    def test_file_exists_and_not_empty_should_return_false_on_non_existing_file(self):
        temp = cc.CURRENCY_RATES_FILE_EXTENSION
        cc.CURRENCY_RATES_FILE_EXTENSION = ".test"
        self.assertEqual(lambda_function.file_exists_and_not_empty(), False)
        cc.CURRENCY_RATES_FILE_EXTENSION = temp

    def test_file_exists_and_not_empty_should_return_false_on_empty_file(self):
        temp = cc.CURRENCY_RATES_FILE_EXTENSION
        cc.CURRENCY_RATES_FILE_EXTENSION = ".test"
        file_name = str(random.randint(1, 100)) + cc.CURRENCY_RATES_FILE_EXTENSION
        with open(file_name, "wt") as file:
            pass
        self.assertEqual(lambda_function.file_exists_and_not_empty(), False)
        os.remove(file_name)
        cc.CURRENCY_RATES_FILE_EXTENSION = temp

    def test_file_exists_and_not_empty_should_return_true(self):
        temp = cc.CURRENCY_RATES_FILE_EXTENSION
        cc.CURRENCY_RATES_FILE_EXTENSION = ".test"
        file_name = (
            "xxx_"
            + str(random.randint(1, 100))
            + "_xxx"
            + cc.CURRENCY_RATES_FILE_EXTENSION
        )
        with open(file_name, "w") as file:
            file.writelines(["test1", "test2"])
        self.assertEqual(lambda_function.file_exists_and_not_empty(), True)
        os.remove(file_name)
        cc.CURRENCY_RATES_FILE_EXTENSION = temp

    def test_is_file_format_valid_should_return_false_on_none_headers(self):
        self.assertEqual(lambda_function.is_file_format_valid(None), False)

    def test_is_file_format_valid_should_return_false_on_many_headers(self):
        lines = ["H201710211400191", "H201710211400192"]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_false_on_none_trailers(self):
        lines = [
            "H201710211400191",
            "D0088402MD000001134066410000001134350000000001134633590999999999999999",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_false_on_many_trailers(self):
        lines = [
            "H201710211400191",
            "T00022200001731625193984",
            "T00022200001731625193984",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_false_on_header_not_being_first_line(
        self,
    ):
        lines = [
            "T00022200001731625193984",
            "H201710211400191",
            "T00022200001731625193984",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_false_on_trailer_not_being_last_line(
        self,
    ):
        lines = [
            "H201710211400191",
            "T00022200001731625193984",
            "D00022200001731625193984",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_false_on_wrong_detail_record_identifier(
        self,
    ):
        lines = [
            "H201710211400191",
            "D0088402MD000001134066410000001134350000000001134633590999999999999999",
            "K0088402MD000001134066410000001134350000000001134633590999999999999999",
            "T00022200001731625193984",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), False)

    def test_is_file_format_valid_should_return_true_on_correct_detail_record(self):
        lines = [
            "H201710211400191",
            "D0088402MD000001134066410000001134350000000001134633590999999999999999",
            "T00022200001731625193984",
        ]
        self.assertEqual(lambda_function.is_file_format_valid(lines), True)

    def test_is_data_record_allowed_should_return_false_on_none_data_record(self):
        self.assertEqual(lambda_function.is_data_record_allowed(None), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_reference_currency_code(
        self,
    ):
        data = DetailRecordModel(978, 978, None, None, None, None, None, None)
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_rate_class(self):
        data = DetailRecordModel(
            978,
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            "F",
            None,
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_unallowed_rate_rate_format_indicator(
        self,
    ):
        data = DetailRecordModel(
            978,
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            "M",
            "P",
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_CNH(self):
        data = DetailRecordModel(
            cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[0],
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            cc.ALLOWED_RATE_CLASSES[0],
            cc.ALLOWED_RATE_FORMAT_INDICATORS[0],
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_CNY(self):
        data = DetailRecordModel(
            978,
            cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[1],
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            cc.ALLOWED_RATE_CLASSES[0],
            cc.ALLOWED_RATE_FORMAT_INDICATORS[0],
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_SVC(self):
        data = DetailRecordModel(
            cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[2],
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            cc.ALLOWED_RATE_CLASSES[0],
            cc.ALLOWED_RATE_FORMAT_INDICATORS[0],
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_false_on_ESA(self):
        data = DetailRecordModel(
            cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES[3],
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            cc.ALLOWED_RATE_CLASSES[0],
            cc.ALLOWED_RATE_FORMAT_INDICATORS[0],
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), False)

    def test_is_data_record_allowed_should_return_true_on_correct_data_record(self):
        data = DetailRecordModel(
            978,
            cc.ALLOWED_REFERENCE_CURRENCY_CODES[0],
            None,
            cc.ALLOWED_RATE_CLASSES[0],
            cc.ALLOWED_RATE_FORMAT_INDICATORS[0],
            None,
            None,
            None,
        )
        self.assertEqual(lambda_function.is_data_record_allowed(data), True)

    def test_convert_file_line_to_header_should_return_none_on_none_line(self):
        self.assertEqual(lambda_function.convert_file_line_to_header(None), None)

    def test_convert_file_line_to_header_should_return_none_on_improper_line_length(
        self,
    ):
        self.assertEqual(lambda_function.convert_file_line_to_header("test"), None)

    def test_convert_file_line_to_header_return_none_on_unsupported_header_format(self):
        self.assertEqual(lambda_function.convert_file_line_to_header("test" * 10), None)

    def test_convert_file_line_to_header_should_return_none_on_improper_date_format(
        self,
    ):
        self.assertEqual(
            lambda_function.convert_file_line_to_header(
                "H15321a15645641fdsa15641fads16546"
            ),
            None,
        )

    def test_convert_file_line_to_header_should_return_header(self):
        line = "H201710211400191                                                                                                             "
        data = lambda_function.convert_file_line_to_header(line)
        date = str(datetime(2017, 10, 21, 14, 0, 19))
        self.assertEqual(data.date, date)

    def test_get_rate_should_raises_exception_on_improper_conversion_rate_length(self):
        with self.assertRaises(Exception):
            lambda_function.convert_exchange_rate(None)

    def test_get_rate_should_raises_exception_on_improper_conversion_rate_length(self):
        with self.assertRaises(Exception):
            lambda_function.convert_exchange_rate("123")

    def test_get_rate_should_raises_exception_on_improper_conversion_rate(self):
        with self.assertRaises(ValueError):
            lambda_function.convert_exchange_rate(
                "t"
                * (
                    cc.CURRENCY_COVERSION_INTEGER_PLACES
                    + cc.CURRENCY_COVERSION_DECIMAL_PLACES
                )
            )

    def test_convert_file_line_to_trailer_should_return_none_on_none_line(self):
        self.assertEqual(lambda_function.convert_file_line_to_trailer(None), None)

    def test_convert_file_line_to_trailer_should_return_none_on_improper_line_length(
        self,
    ):
        self.assertEqual(lambda_function.convert_file_line_to_trailer("test"), None)

    def test_convert_file_line_to_trailer_should_return_none_on_improper_line_format(
        self,
    ):
        self.assertEqual(lambda_function.convert_file_line_to_header("test" * 10), None)

    def test_convert_file_line_to_trailer_should_return_trailer(self):
        line = "T00022200001731625193984                                                                                                     "
        data = lambda_function.convert_file_line_to_trailer(line)
        self.assertEqual(data.total_records, 222)
        self.assertEqual(data.hash_total, 1731625193984)

    def test_convert_file_line_to_detail_record_should_return_none_on_none_line(self):
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(None), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_line_length(
        self,
    ):
        self.assertEqual(
            lambda_function.convert_file_line_to_detail_record("test"), None
        )

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_source_currency_code(
        self,
    ):
        line = "Dana8402MD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_reference_currency_code(
        self,
    ):
        line = "D555ana2MD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_source_currency_exponent(
        self,
    ):
        line = "D555777pMD000013674316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_buy_currency_conversion_rate(
        self,
    ):
        line = "D5557772MD0000136p4316250000013675000000000013675683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_mid_currency_conversion_rate(
        self,
    ):
        line = "D5557772MD0000136243162500000136p5000000000013675683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_none_on_improper_sell_currency_conversion_rate(
        self,
    ):
        line = "D5557772MD0000136543162500000136750000000000136p5683750999999999999999                                                       "
        self.assertEqual(lambda_function.convert_file_line_to_detail_record(line), None)

    def test_convert_file_line_to_detail_record_should_return_detail_record(self):
        line = "D5557772MD000013654316250000013675000000000013675683750999999999999999                                                       "
        data = lambda_function.convert_file_line_to_detail_record(line)
        self.assertEqual(data.source_currency_code, 555)
        self.assertEqual(data.reference_currency_code, 777)
        self.assertEqual(data.source_currency_exponent, 2)
        self.assertEqual(data.rate_class, "M")
        self.assertEqual(data.rate_format_indicator, "D")
        self.assertEqual(data.buy_currency_conversion_rate, 1365.431625)
        self.assertEqual(data.mid_currency_conversion_rate, 1367.5)
        self.assertEqual(data.sell_currency_conversion_rate, 1367.568375)

    def test_is_file_name_valid_should_return_false_on_incorrect_file_name(self):
        file_name = "test_name"
        data = lambda_function.is_file_name_valid(file_name)
        self.assertEqual(data, False)

    def test_is_file_name_valid_should_return_true_on_correct_file_name(self):
        file_name = "I_171022_T057.sw0"
        data = lambda_function.is_file_name_valid(file_name)
        self.assertEqual(data, True)

    def test_set_currency_rates_file_name_should_set_latest_file_name(self):
        file_names = ["I_171022_T057.sw0", "I_171025_T057.sw0", "I_171019_T057.sw0"]
        lambda_function.set_currency_rates_file_name(file_names)
        self.assertEqual(lambda_function.currency_rates_file_name, "I_171025_T057.sw0")


if __name__ == "__main__":
    unittest.main()
