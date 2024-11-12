from EventSystem import event_system

class LongTermAnalysisModule:
    def __init__(self):
        event_system.subscribe(self.process_data, 'message_received')

    def process_data(self, data):
        pass