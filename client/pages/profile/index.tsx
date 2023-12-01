import React, {RefObject, useCallback, useEffect, useRef, useState} from "react";
import Input from "@/components/Input";
import {extractCookieAuthenticationData} from "@/src/services/token";
import {getUser, removeUser, updateUser} from "@/src/services/users";
import Button from "@/components/Button";
import Toggle from "@/components/Toggle";
import DeleteIcon from "@/components/icons/DeleteIcon";
import Dialog, {DialogModalHandle} from "@/components/Dialog";
import Navbar from "@/components/general/Navbar";
import Title from "@/components/general/Title";
import Subtitle from "@/components/general/Subtitle";
import Description from "@/components/general/Description";
import {useQRCode} from "next-qrcode";
import {useRouter} from "next/router";
import {registerHotp, registerTotp} from "@/src/services/auth/otp";
import {
	buildPasskeyRegistrationConfiguration,
	confirmPasskeyRegistration,
	createPasskeyAttestation
} from "@/src/services/auth/passkeys";
import {User, UserUnauthorizedResponse} from "@/src/models/user";
import {RegisterOtpResponse} from "@/src/models/auth/otp";

type ProfileProps = {
	user: User,
	token: string
}

export default function Profile({user, token}: ProfileProps) {
	const [username, setUsername] = useState<string>(user.username);
	const [mfa, setMfa] = useState<boolean>(user.credentials.mfa);

	const [otp, setOtp] = useState<RegisterOtpResponse | undefined>();

	const hasOtp = useCallback(() => user.credentials.otp.hashCodes.length > 0 || user.credentials.otp.timeCodes.length > 0, [user.credentials.otp])

	const otpDialog = useRef<DialogModalHandle>(null);
	const router = useRouter();

	async function hotpRegister() {
		const response = await registerHotp(user?.uuid, token)
		setOtp(response)
	}

	async function totpRegister() {
		const response = await registerTotp(user?.uuid, token)
		setOtp(response)
	}

	async function passkeyRegister() {
		const configuration = await buildPasskeyRegistrationConfiguration(user?.uuid, token);
		const credential = await createPasskeyAttestation(configuration)
		if (credential === null)
			throw 'The given credential from passkey registration is invalid.'

		await confirmPasskeyRegistration(user?.uuid, credential, token)

		router.reload();
	}

	async function deleteAccount() {
		await removeUser(user.uuid, token)

		router.reload();
	}

	async function update() {
		if (user.username === username && user.credentials.mfa === mfa)
			return;

		await updateUser(user.uuid, token, mfa, username)
	}

	useEffect(() => {
		if (otp === undefined)
			return;

		otpDialog.current?.show();
	}, [otp]);

	useEffect(() => {
		(async () => await update())();
	}, [mfa]);

	return (
		<>
			<div className={"flex flex-col p-8 w-[50%] bg-white rounded-xl shadow-lg"}>
				<div className={"flex flex-col gap-4"}>
					<Title>
						Profile
					</Title>

					<Subtitle>
						Details
					</Subtitle>

					<Input
						value={username}
						onBlur={update}
						onChange={value => setUsername(value.currentTarget.value)}
						minimumLength={3}
						name={"username"}
						title={"Username"}
						placeholder={"Alice"}
					/>

					<Input
						value={user.email}
						name={"email"}
						title={"Email"}
						placeholder={"test@account.com"}
						disabled
					/>

					<Input
						value={user.uuid}
						name={"uuid"}
						title={"Id"}
						placeholder={"0000-0000-0000-0000"}
						disabled
					/>

				</div>
				<OtpDialog
					dialog={otpDialog}
					secret={otp?.secret}
					uri={otp?.uri}
				/>
			</div>
			<div className={"flex flex-col p-8 w-[50%] bg-white rounded-xl shadow-lg"}>
				<div className={"flex flex-col gap-4"}>
					<Title>
						Management
					</Title>

					<Subtitle>
						Multi-factor
					</Subtitle>

					<Toggle>
						<Toggle.Option
							id={"mfa"}
							value={mfa}
							onChange={value => setMfa(value.currentTarget.checked)}
							disabled={!hasOtp()}
						/>
						Multi-factor enabled
					</Toggle>

					<div className={"flex flex-col gap-2"}>
						<Button onClick={hotpRegister} title={"Register HOTP"}/>
						<Button onClick={totpRegister} title={"Register TOTP"}/>
					</div>

					<Subtitle>
						Passkeys
					</Subtitle>
					<Button onClick={passkeyRegister} title={"Register passkey"}/>

					<Subtitle>
						General
					</Subtitle>
					<Button onClick={deleteAccount} style={"tertiary"} title={"Delete account"} icon={<DeleteIcon className={"text-white w-5 h-5"}/>}/>
				</div>
			</div>
			<Navbar selectedMenu={"profile"}/>
		</>
	)
}

type OtpDialogProps = {
	dialog: RefObject<DialogModalHandle>,
	secret: string | undefined,
	uri: string | undefined
}

function OtpDialog({dialog, secret, uri}: OtpDialogProps) {
	const {Canvas: QRCode} = useQRCode();

	return (
		<Dialog.Modal ref={dialog}>
			<Dialog.Container>
				<Dialog.Column>
					<Title context={"dialog"}>
						Register code
					</Title>

					<div className={"max-w-[256px]"}>
						<Description>
							{secret}
						</Description>
					</div>

					<QRCode
						text={uri ?? ""}
						options={{
							margin: 0,
							width: 256
						}}
					/>
				</Dialog.Column>
			</Dialog.Container>
		</Dialog.Modal>
	)
}

export async function getServerSideProps({req}: any) {
	const [token, uuid] = extractCookieAuthenticationData(req.headers.cookie);

	try {
		const isUserUnauthorizedResponse = (u: any): u is UserUnauthorizedResponse => u.url !== undefined;

		const user = await getUser(uuid, token);
		const isUnauthorizedResponse = isUserUnauthorizedResponse(user);

		if (isUnauthorizedResponse)
			return {
				redirect: {
					permanent: false,
					destination: "/mfa",
				}
			}

		return {props: {user, token}};
	} catch (e) {
		return {
			redirect: {
				permanent: false,
				destination: "/login",
			}
		}
	}
}