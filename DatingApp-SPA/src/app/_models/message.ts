export interface Message {
    id: number;
    senderId: number;
    senderKnownAs: number;
    senderPhotoUrl: string;
    recipientId: number;
    recipientKnownAs: number;
    recipientPhotoUrl: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}
