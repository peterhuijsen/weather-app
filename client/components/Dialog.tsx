"use client"

import React, {forwardRef, useImperativeHandle, useState} from "react";
import {AnimatePresence, motion} from "framer-motion";

/*
Used to group together common components for dialog functionality in the application.

Instead of the default usage of 'function' declaration for components, 'const' is used because
we need to assign properties to it, like modals, containers, buttons, etc.
 */
const Dialog = () => { return (<></>); }

/*
The types of functions available to dialog modals.
 */
export type DialogModalHandle = {
	/* Make the dialog modal appear. */
	show: () => void,
	/* Make the dialog modal disappear. */
	hide: () => void,
}

/*
The properties of dialog modals.
 */
type DialogModalProps = {
	/* Called when the dialog modal is closed by tapping the back overlay. */
	onClose?: () => void,
	/* The children of the dialog modal component. */
	children: React.ReactNode,
}

/*
Used to display a modal dialog to the user, with a back overlay to grab the user's attention.
 */
const DialogModal = forwardRef<DialogModalHandle, DialogModalProps>(({ onClose, children } : DialogModalProps, ref) => {
	/* Whether or not the dialog modal is shown to the user. */
	const [isShown, setIsShown] = useState<boolean>(false);

	/*
	Hide the modal and call the given callback function.
	 */
	function onCloseModal() {
		setIsShown(false);
		if (onClose) onClose();
	}

	/*
	Define the functions available to references to the modal.
	 */
	useImperativeHandle(ref, () => {
		return {
			show: () => setIsShown(true),
			hide: () => setIsShown(false),
		}
	})

	return (
		<AnimatePresence>
			{isShown && (
				<div className={"flex"}>
					<motion.div
						onClick={onCloseModal}
						className={"fixed top-0 left-0 w-full h-full bg-neutral-900 z-50 bg-opacity-30"}
						transition={{ duration: 0.1 }}
						initial={{ opacity: 0 }}
						animate={{ opacity: 1 }}
						exit={{ opacity: 0 }}
					/>
					<motion.div
						className={"fixed top-0 left-0 w-full h-full z-50 pointer-events-none"}
						transition={{ duration: 0.1 }}
						initial={{ opacity: 0.6 }}
						animate={{ opacity: 1 }}
						exit={{ opacity: 0 }}
					>
						<div className={"flex w-full h-full p-8 justify-center items-center"}>
							<div className={"flex flex-col max-w-full gap-4 p-6 bg-white rounded-2xl pointer-events-auto shadow-2xl"}>
								{children}
							</div>
						</div>
					</motion.div>
				</div>
			)}
		</AnimatePresence>
	)
});

/*
The properties of dialog forms.
 */
type DialogFormProps = {
	onSubmit: (event: any) => void;
	children: React.ReactNode
}

/*
Used to define a constant style of form which should be used in dialog modals.
 */
const DialogForm = ({ onSubmit, children } : DialogFormProps) => {
	return (
		<form onSubmit={onSubmit} className={"flex flex-col gap-[24px]"}>
			{children}
		</form>
	)
}

/*
The properties of dialog sections (columns, rows, etc.).
 */
type DialogSectionProps = {
	children: React.ReactNode
}

/*
Used to define a constant style of vertical item separation which should be used in dialog modals.
 */
const DialogColumn = ({ children } : DialogSectionProps) => {
	return (
		<div className={"flex flex-col gap-[12px]"}>
			{children}
		</div>
	)
}

/*
Used to define a constant style of horizontal item separation which should be used in dialog modals.
 */
const DialogRow = ({ children } : DialogSectionProps) => {
	return (
		<div className={"flex flex-row gap-[12px]"}>
			{children}
		</div>
	)
}

/*
The properties of dialog containers (columns, rows, etc.).
 */
type DialogContainerProps = {
	children: React.ReactNode
}

/*
Used to define a constant style of containers in dialog modals.
 */
const DialogContainer = ({ children } : DialogContainerProps) => {
	return (
		<div className={"flex flex-col gap-[24px]"}>
			{children}
		</div>
	)
}

// Assign all the components to the properties of the dialog object.
Dialog.Modal = DialogModal;
Dialog.Form = DialogForm;
Dialog.Column = DialogColumn;
Dialog.Row = DialogRow;
Dialog.Container = DialogContainer;

// Export the dialog object.
export default Dialog;