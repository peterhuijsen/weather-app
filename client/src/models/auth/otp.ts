export type RegisterOtpResponse = {
	secret: string,
	uri: string,
	counter: number
}

export type VerifyOtpResponse = {
	verified: boolean,
	counter: number
}

export type Otp = {
	hashCodes: HashOtp[],
	timeCodes: TimeOtp[]
}

export type HashOtp = {
	counter: string
}

export type TimeOtp = { }