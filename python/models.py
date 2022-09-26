class HeaderModel:
    """
    Class representing header row in exchange rates file.
    """

    def __init__(self, date):
        self.date = date


class DetailRecordModel:
    """
    Class representing detail record row in exchange rates file.
    """

    def __init__(
        self,
        source_currency_number,
        reference_currency_number,
        source_currency_exponent,
        rate_class,
        rate_format_indicator,
        buy_currency_conversion_rate,
        mid_currency_conversion_rate,
        sell_currency_conversion_rate,
    ):
        self.source_currency_code = source_currency_number
        self.reference_currency_code = reference_currency_number
        self.source_currency_exponent = source_currency_exponent
        self.rate_class = rate_class
        self.rate_format_indicator = rate_format_indicator
        self.buy_currency_conversion_rate = buy_currency_conversion_rate
        self.mid_currency_conversion_rate = mid_currency_conversion_rate
        self.sell_currency_conversion_rate = sell_currency_conversion_rate


class TrailerModel:
    """
    Class representing trailer row in exchange rates file.
    """

    def __init__(self, total_records, hash_total):
        self.total_records = total_records
        self.hash_total = hash_total


class RequestModel:
    """
    Class representing model which contains exchange rates data from
    the file, which are going to be sent to API.
    """

    def __init__(self, header, detail_records, trailer, error):
        self.header = header
        self.detail_records = detail_records
        self.trailer = trailer
        self.error = error
